using Common.Classes;
using Common.ClassesJSON;
using Common.JsonSourceGenerators;
using ICSharpCode.SharpZipLib.Tar;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace Common.Handlers
{
    public class JavaScriptCheckHandler
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
                if (entry.Name.Trim().EndsWith(".js"))
                {
                    var jsFile = new JSFile(entry);
                    extension.ContainedJSFiles.Add(jsFile);
                }
            }
            extension.ContainedJSFiles = [.. extension.ContainedJSFiles.DistinctBy(x => x.LenChecksum).OrderBy(x => x.GetFullName())];
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
            var client2 = new HttpClient();

            Parallel.ForEach(jsFile.NPMRegistries, (registry,state) =>
            {
                try
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    var result = _httpClient.GetStringAsync(Res.Params.NPMRegistryGetURL.Replace("[NAME]", registry.Name)).Result;

                    if (!string.IsNullOrEmpty(result))
                    {
                        var queryResult = JsonSerializer.Deserialize(result, NpmPackageSG.Default.NpmPackageJson) ?? new NpmPackageJson();

                        registry.LatestVersionStable = queryResult.DistributionTags.LatestVersionStable;
                        registry.LatestVersionDevelopment = queryResult.DistributionTags.LatestVersionDevelopment;

                        DateTime versionDate = DateTime.MinValue;

                        if (queryResult.DateVersions.TryGetValue(registry.LatestVersionStable, out versionDate))
                        {
                            registry.LatestUpdateStable = versionDate;
                        }

                        if (queryResult.DateVersions.TryGetValue(registry.LatestVersionDevelopment, out versionDate))
                        {
                            registry.LatestUpdateDevelopment = versionDate;
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
                            registry.Packages.Add(package);
                        }

                        Parallel.ForEach(registry.Packages, () => new JSCheckBufferBag(), (package, state, sPair) =>
                        {
                            try
                            {
                                using (var httpStream = _httpClient.GetStreamAsync(package.TarballUrl).Result)
                                {
                                    httpStream.CopyTo(sPair.DownloadStream);
                                }
                                GetTarEntries(sPair, package, jsFile);
                                sPair.Clear();

                                if (package.BestSimilarity == 1.0)
                                {
                                    sPair.Close();
                                    state.Break();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            return sPair;
                        }, (sPair) =>
                        {
                            sPair.Close();
                        });

                        registry.CountNumFilesChecked();

                        if (registry.HasAnyMatchedPackages())
                        {
                            double hPackSimilarity = -1;

                            foreach (var package in registry.Packages)
                            {
                                if (package.BestSimilarity > hPackSimilarity)
                                {
                                    hPackSimilarity = package.BestSimilarity;
                                    registry.BestPackage = package;
                                }
                            }
                            registry.DisposePackagesList();

                            if (hPackSimilarity == 1.0)
                            {
                                state.Break();
                            }
                        }
                    }

                    watch.Stop();
                    var time = watch.ElapsedMilliseconds / 1000.0;

                    Console.WriteLine("{0} analyzed - {1} files after {2:0.00}s", registry.Name, registry.TotalNumFilesChecked, time);
                }
                catch(Exception e) 
                {
                    Console.WriteLine(e.Message);
                }
            });

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

            jsFile.DisposeRegistriesList();
            jsFile.DisposeProfile();

            Console.WriteLine(jsFile.ToString());
            return true;
        }
        private static void GetTarEntries(JSCheckBufferBag sPair, NPMPackage package, JSFile jsFile)
        {
            int filesChecked = 0;

            var tarStream = sPair.SetTarReading();

            while (tarStream.GetNextEntry() is TarEntry entry)
            {
                if(!entry.IsDirectory)
                {
                    filesChecked++;
                    if (entry.Name.EndsWith(".js"))
                    {
                        double sizeRatio = (double)entry.Size / jsFile.Lenght;

                        if (sizeRatio < 1.15 && sizeRatio > 0.85)
                        {
                            var content = sPair.GetEntry(tarStream);

                            package.UpdateSimilarity(SimilarityHandler.GetSimilarity(jsFile, content, sPair.ContentProfile));

                            if (package.BestSimilarity == 1.0)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            package.FilesChecked = filesChecked;
        }
    }
}