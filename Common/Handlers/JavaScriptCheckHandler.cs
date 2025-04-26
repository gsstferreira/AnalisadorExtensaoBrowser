using Amazon.Lambda;
using Common.Classes;
using Common.ClassesJSON;
using Common.JsonSourceGenerators;
using SharpZipLib.Tar;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Common.Handlers
{
    public abstract class JavaScriptCheckHandler
    {
        private const int _sleep_npm_queries = 1000;
        private static readonly HttpClient _httpClient = new();

        public static void CheckJSFiles(BrowserExtension extension)
        {
            GetJSFiles(extension);
            var numFiles = extension.ContainedJSFiles.Count;
            Console.WriteLine(string.Format("{0} - {1} arquivos JavaScript encontrados na extensão", extension.Name, numFiles));

            var bag = new ConcurrentStack<JSFile>();

            var queryTask = new Task<int>(() =>
            {
                int totalFilesChecked = 0;
                foreach (JSFile jsFile in extension.ContainedJSFiles)
                {
                    var elapsedTime = GetPotentialNPMPackages(jsFile);
                    totalFilesChecked++;
                    bag.Push(jsFile);
                    var remainingTime = _sleep_npm_queries - (int)elapsedTime;
                    if (remainingTime > 0)
                    {
                        Thread.Sleep(remainingTime);
                    }
                }
                return totalFilesChecked;
            });

            var validationTask = new Task<int>(() =>
            {
                int totalFilesChecked = 0;
                while (totalFilesChecked != numFiles)
                {
                    if (!bag.TryPop(out var jsFile))
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        if (jsFile.NPMRegistries.Count > 0)
                        {
                            ValidateNPMPackages(jsFile);
                        }
                        else
                        {
                            Console.WriteLine(jsFile.ToString());
                        }
                        totalFilesChecked++;
                    }
                }
                return totalFilesChecked;
            });

            queryTask.Start();
            validationTask.Start();

            queryTask.Wait();
            validationTask.Wait();

            Console.WriteLine("{0}:{1}:{2}", numFiles, queryTask.Result, validationTask.Result);
        }
        public static void GetJSFiles(BrowserExtension extension)
        {
            foreach (var entry in extension.ExtensionContent.Entries)
            {
                bool isRight = entry.Name.Contains("analytics") || entry.Name.Contains("jquery") || entry.Name.Contains("readability");
                if (entry.Name.Trim().EndsWith(".js") && entry.Name.Contains("analytics"))
                {
                    var jsFile = new JSFile(entry);
                    extension.ContainedJSFiles.Add(jsFile);
                }
            }
            extension.ContainedJSFiles = [.. extension.ContainedJSFiles.DistinctBy(x => x.Crc32).OrderBy(x => x.FullName)];
        }


        private static long GetPotentialNPMPackages(JSFile jsFile)
        {
            Stopwatch sw = new();
            sw.Start();

            try
            {
                var jsName = jsFile.Name.EndsWith(".min") ? jsFile.Name.Replace(".min", string.Empty) : jsFile.Name;

                var result = _httpClient.GetStringAsync(Res.Params.NPMRegistryQueryURL.Replace("[NAME]", jsName)).Result;

                if (!string.IsNullOrEmpty(result))
                {
                    var qResult = JsonSerializer.Deserialize(result, NpmQuerySG.Default.NpmQueryJSON) ?? new NpmQueryJSON();

                    foreach (var item in qResult.QueryResults)
                    {
                        var registryName = item.Registry.Name;

                        if (registryName.Contains(jsName) || jsName.Contains(registryName))
                        {
                            var registry = new NpmRegistry
                            {
                                Name = registryName,
                                HomepageUrl = item.Registry.Links.Homepage,
                                RepositoryUrl = item.Registry.Links.Repository,
                                BugTrackerUrl = item.Registry.Links.BugTracker,
                                NPMPageUrl = item.Registry.Links.Npm,
                            };
                            jsFile.NPMRegistries.Add(registry);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        private static bool ValidateNPMPackages(JSFile jsFile)
        {
            if (jsFile.NPMRegistries.Count > 0)
            {
                SimilarityHandler.SetCosineProfile(jsFile);

                // Obtendo os pacotes npm para cada registro encontrado
                Parallel.ForEach(jsFile.NPMRegistries, reg =>
                {
                    var result = _httpClient.GetStringAsync(Res.Params.NPMRegistryGetURL.Replace("[NAME]", reg.Name)).Result;

                    if (!string.IsNullOrEmpty(result))
                    {
                        var queryResult = JsonSerializer.Deserialize(result, NpmPackageSG.Default.NpmPackageJson) ?? new NpmPackageJson();

                        reg.LatestVersionStable = queryResult.DistributionTags.LatestVersionStable;
                        reg.LatestVersionDevelopment = queryResult.DistributionTags.LatestVersionDevelopment;

                        DateTime versionDate = DateTime.MinValue;

                        if (queryResult.DateVersions.TryGetValue(reg.LatestVersionStable, out versionDate))
                        {
                            reg.LatestUpdateStable = versionDate;
                        }

                        if (queryResult.DateVersions.TryGetValue(reg.LatestVersionDevelopment, out versionDate))
                        {
                            reg.LatestUpdateDevelopment = versionDate;
                        }

                        foreach (var version in queryResult.Versions)
                        {
                            var package = new NPMPackage
                            {
                                Name = version.Value.Name,
                                Version = version.Key,
                                ReleaseDate = queryResult.DateVersions[version.Key],
                                TarballUrl = version.Value.DstributionDetails.TarballUrl
                            };
                            reg.Packages.Add(package);
                        }
                    }
                });
                var regTasks = new List<Task>();
                
                int profDictionaryCap = (int)(jsFile.Lenght / SimilarityHandler.DEFAULT_K);

                foreach (var reg in jsFile.NPMRegistries)
                {
                    regTasks.Add(Task.Factory.StartNew(() => 
                    {
                        var packTasks = new List<Task>();
                        var cancelToken = new CancellationTokenSource();

                        if (reg.Packages.Count > 10)
                        {
                            var newList = new List<NPMPackage>(reg.Packages);

                            while (newList.Count > 0)
                            {
                                var iterList = newList.Take(10).ToList();
                                newList = [.. newList.Skip(10)];

                                packTasks.Add(Task.Factory.StartNew(() =>
                                {
                                    var bufferBag = new JSCheckBufferBag(profDictionaryCap);
                                    foreach (var package in iterList)
                                    {
                                        try
                                        {
                                            using (var httpStream = _httpClient.GetStreamAsync(package.TarballUrl).Result)
                                            {
                                                httpStream.CopyTo(bufferBag.DownloadStream);
                                            }
                                            GetTarEntries(bufferBag, package, jsFile);
                                        }
                                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                                        finally
                                        {
                                            if (package.BestSimilarity == 1.0)
                                            {
                                                cancelToken.Cancel();
                                            }
                                            bufferBag.Clear();
                                        }
                                    }
                                    bufferBag.Close();
                                }, cancelToken.Token));
                            }
                        }
                        else
                        {
                            foreach (var package in reg.Packages)
                            {
                                packTasks.Add(Task.Factory.StartNew(() =>
                                {
                                    var bufferBag = new JSCheckBufferBag(profDictionaryCap);

                                    try
                                    {
                                        using (var httpStream = _httpClient.GetStreamAsync(package.TarballUrl).Result)
                                        {
                                            httpStream.CopyTo(bufferBag.DownloadStream);
                                        }
                                        GetTarEntries(bufferBag, package, jsFile);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                    finally
                                    {
                                        bufferBag.Close();
                                    }
                                    if (package.BestSimilarity == 1.0)
                                    {
                                        cancelToken.Cancel();
                                    }
                                }, cancelToken.Token));
                            }
                        }

                        Task.WaitAll([.. packTasks]);
                        packTasks.Clear();
                        cancelToken.Dispose();
                        if (reg.HasAnyMatchedPackages())
                        {
                            double hPackSimilarity = -1;

                            foreach (var package in reg.Packages)
                            {
                                if (package.BestSimilarity > hPackSimilarity)
                                {
                                    hPackSimilarity = package.BestSimilarity;
                                    reg.BestPackage = package;
                                }
                            }
                            reg.DisposePackagesList();
                        }
                    }));
                }
                Task.WaitAll([.. regTasks]);
                regTasks.Clear();
 
                double highestSimilarity = -1;

                foreach (var registry in jsFile.NPMRegistries)
                {
                    jsFile.TotalFilesChecked += registry.TotalNumFilesChecked;

                    if (registry.BestPackage is not null)
                    {
                        var sim = registry.BestPackage.BestSimilarity;
                        highestSimilarity = sim > highestSimilarity ? sim : highestSimilarity;
                        jsFile.BestRegistry = registry;
                    }
                }
            }

            jsFile.DisposeRegistriesList();
            jsFile.DisposeProfile();

            Console.WriteLine(jsFile.ToString());
            return true;
        }
        private static void GetTarEntries(JSCheckBufferBag sPair, NPMPackage package, JSFile jsFile)
        {
            var tarStream = sPair.SetTarReading();
            while (tarStream.GetNextEntry() is TarEntry entry)
            {
                if (!entry.IsDirectory)
                {
                    package.FilesChecked++;
                    if (IsComparableSize(entry.Size, jsFile.Lenght))
                    {
                        if (IsJsFile(entry))
                        {
                            var content = tarStream.GetContentAsString();
                            package.UpdateSimilarity(SimilarityHandler.GetSimilarity(jsFile, content, sPair.ContentProfile));

                            if (package.BestSimilarity == 1.0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsComparableSize(long size1, long size2)
        {
            var sizeRatio = (double)size1 / size2;

            return sizeRatio < 1.15 && sizeRatio > 0.85;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //Implementação manual de string.EndsWith(".js") para melhorias de desempenho
        private static bool IsJsFile(TarEntry entry)
        {
            if (entry.Name.Length > 3)
            {
                if (entry.Name[^1] == 's')
                {
                    if (entry.Name[^2] == 'j')
                    {
                        if (entry.Name[^3] == '.')
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }
    }
}