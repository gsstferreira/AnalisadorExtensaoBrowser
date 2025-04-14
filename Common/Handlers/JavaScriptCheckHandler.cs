using Common.Classes;
using Common.ClassesJSON;
using ICSharpCode.SharpZipLib.Tar;
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
            Console.WriteLine(string.Format("{0} arquivos JavaScript encontrados na extensão", numFiles));

            var tasks = new List<Task<bool>>();

            foreach (var file in extension.ContainedJSFiles) 
            {
                var elapsedTime = GetPotentialNPMPackages(file);
                if (file.NPMRegistries.Count > 0)
                {
                    var task = new Task<bool>(() => ValidateNPMPackages(file));
                    tasks.Add(task);
                    task.Start();
                }
                else
                {
                    Console.WriteLine(file.ToString());
                }

                var remainingTime = _sleep_npm_queries - (int)elapsedTime;
                if (remainingTime > 0)
                {
                    Thread.Sleep(remainingTime);
                }
            }
            Task.WhenAll(tasks);
        }
        public static void GetJSFiles(BrowserExtension extension)
        {
            foreach (var entry in extension.CrxArchive.Entries)
            {
                if (entry.Name.EndsWith(".js"))
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
                    var queryResult = JsonSerializer.Deserialize<NpmQueryJSON>(result) ?? new NpmQueryJSON();

                    foreach (var item in queryResult.QueryResults)
                    {
                        var registryName = item.Registry.Name;

                        if(registryName.Contains(jsName) || jsName.Contains(registryName)) 
                        {
                            var registry = new NPMRegistry
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
            Parallel.ForEach(jsFile.NPMRegistries, registry =>
            {
                var result = _httpClient.GetStringAsync(Res.Params.NPMRegistryGetURL.Replace("[NAME]", registry.Name)).Result;
                if (!string.IsNullOrEmpty(result))
                {
                    var queryResult = JsonSerializer.Deserialize<NpmPackageJson>(result) ?? new NpmPackageJson();

                    registry.LatestVersionStable = queryResult.DistributionTags.LatestVersionStable;
                    registry.LatestVersionDevelopment = queryResult.DistributionTags.LatestVersionDevelopment;

                    DateTime versionDate = DateTime.MinValue;

                    if(queryResult.DateVersions.TryGetValue(registry.LatestVersionStable, out versionDate))
                    {
                        registry.LatestUpdateStable = versionDate;
                    }

                    if(queryResult.DateVersions.TryGetValue(registry.LatestVersionDevelopment, out versionDate))
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
                }
                //registry.Packages = [.. registry.Packages.OrderByDescending(p => p.ReleaseDate)];
            });

            double currentHighestSimilarity = 0;

            foreach (var registry in jsFile.NPMRegistries)
            {
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

                if (registry.HasAnyMatchedPackages())
                {
                    double hPackSimilarity = -1;

                    foreach (var package in registry.Packages)
                    {
                        if (package.BestSimilarity > hPackSimilarity)
                        {
                            hPackSimilarity = package.BestSimilarity;
                            registry.MostSimilarPackage = package;
                        }
                    }
                    registry.DisposePackagesList();

                    if (hPackSimilarity > currentHighestSimilarity)
                    {
                        currentHighestSimilarity = hPackSimilarity;
                        jsFile.BestMatchedRegistry = registry;
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
            var tarInputStream = sPair.GetTarStream();

            while (tarInputStream.GetNextEntry() is TarEntry entry)
            {
                if (entry.Name.EndsWith(".js"))
                {
                    jsFile.TotalFilesChecked++;
                    double sizeRatio = (double) entry.Size / jsFile.Lenght;

                    if (sizeRatio < 1.15 && sizeRatio > 0.85)
                    {
                        var content = sPair.GetEntryContents(tarInputStream);
                        package.UpdateSimilarity(SimilarityHandler.GetSimilarity(jsFile, content));
                    }
                }
            }
        }
    }
}