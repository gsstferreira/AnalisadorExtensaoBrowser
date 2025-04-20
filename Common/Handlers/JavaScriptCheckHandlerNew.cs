using Common.Classes;
using Common.ClassesJSON;
using Common.JsonSourceGenerators;
using ICSharpCode.SharpZipLib.Tar;
using System.Diagnostics;
using System.Text.Json;

namespace Common.Handlers
{
    public class JavaScriptCheckHandlerNew
    {
        private static readonly HttpClient _httpClient = new();

        public static List<JSFile> GetJSFiles(BrowserExtension extension)
        {
            var list = new List<JSFile>();

            foreach (var entry in extension.ExtensionContent.Entries)
            {
                if (entry.Name.Trim().EndsWith(".js"))
                {
                    list.Add(new JSFile(entry));
                }
            }
            return [.. list.DistinctBy(x => x.LenChecksum).OrderBy(x => x.GetFullName())];
        }

        public static long GetPotentialNPMPackages(JSFile jsFile)
        {
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var jsName = jsFile.Name.EndsWith(".min") ? jsFile.Name.Replace(".min", string.Empty) : jsFile.Name;

                var result = _httpClient.GetStringAsync(Res.Params.NPMRegistryQueryURL.Replace("[NAME]", jsName)).Result;

                if (!string.IsNullOrEmpty(result))
                {
                    var queryResult = JsonSerializer.Deserialize(result, NpmQuerySG.Default.NpmQueryJSON) ?? new NpmQueryJSON();

                    foreach (var item in queryResult.QueryResults)
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
        public static void ValidateNPMPackages(JSFile jsFile)
        { 

            if (jsFile.NPMRegistries.Count > 0) 
            {
                var sw = Stopwatch.StartNew();
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

                // Analisando os pacotes de cada registro
                Parallel.ForEach(jsFile.NPMRegistries, (reg,state) =>
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    Parallel.ForEach(reg.Packages, () => new JSCheckBufferBag(), (package, state, sPair) =>
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

                    reg.CountNumFilesChecked();

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

                        if (hPackSimilarity == 1.0)
                        {
                            state.Break();
                        }
                    }
                    var elapsed = stopwatch.ElapsedMilliseconds/1000.0;

                    Console.WriteLine("[{0}] - ({1}) - {2} arquivos em {3:0.00}s", jsFile.Name, reg.Name, reg.TotalNumFilesChecked, elapsed);
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

                sw.Stop();

                var elapsed = sw.ElapsedMilliseconds / 1000.0;

                Console.WriteLine("[{0}] - Todos os registros analisados em {1:0.00}s", jsFile.Name, elapsed);
            }            
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