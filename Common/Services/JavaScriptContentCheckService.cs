using Common.Classes;
using Common.ClassesJSON;
using ICSharpCode.SharpZipLib.Tar;
using System.Diagnostics;
using System.Text.Json;

namespace Common.Services
{
    public class JavaScriptContentCheckService
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
        private static void GetJSFiles(BrowserExtension extension)
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
                        var packName = item.Package.Name;

                        if(packName.Contains(jsName) || jsName.Contains(packName)) 
                        {
                            var package = new NPMRegistry
                            {
                                Name = packName,
                                HomepageUrl = item.Package.Links.Homepage,
                                RepositoryUrl = item.Package.Links.Repository,
                                BugTrackerUrl = item.Package.Links.BugTracker,
                                NPMPageUrl = item.Package.Links.Npm,
                            };
                            jsFile.NPMRegistries.Add(package);
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
                registry.Packages = [.. registry.Packages.OrderByDescending(p => p.ReleaseDate)];
            });

            foreach (var registry in jsFile.NPMRegistries)
            {
                double highestSimilarity = 0;
                
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
                    var latest = registry.Packages.First();
                    registry.LatestVersion = latest.Version;
                    registry.LastUpdated = latest.ReleaseDate;

                    double hPackSimilarity = 0;

                    foreach (var package in registry.Packages)
                    {
                        if (package.BestSimilarity > hPackSimilarity)
                        {
                            hPackSimilarity = package.BestSimilarity;
                            registry.MostSimilarPackage = package;
                        }
                    }
                    registry.DisposePackagesList();
                }

                if (registry.MostSimilarPackage != null)
                {
                    if (registry.MostSimilarPackage.BestSimilarity > highestSimilarity)
                    {
                        highestSimilarity = registry.MostSimilarPackage.BestSimilarity;
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
                        package.UpdateSimilarity(CosineSimilarityService.GetSimilarity(jsFile, content));
                    }
                }
            }
        }
    }
}