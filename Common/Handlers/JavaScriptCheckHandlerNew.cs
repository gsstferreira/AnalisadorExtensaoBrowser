using Amazon.Lambda;
using Amazon.Runtime;
using Common.Classes;
using Common.ClassesJSON;
using Common.JsonSourceGenerators;
using Res;
using SharpZipLib.Tar;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
            return [.. list.DistinctBy(x => x.FullName).DistinctBy(x => x.Crc32).OrderBy(x => x.FullName)];
        }

        public static long GetPotentialNPMPackages(JSFile jsFile)
        {
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var jsName = jsFile.Name.EndsWith(".min") ? jsFile.Name.Replace(".min", string.Empty) : jsFile.Name;

                var result = _httpClient.GetStringAsync(Params.NPMRegistryQueryURL.Replace("[NAME]", jsName)).Result;

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
                Console.WriteLine("[{0}] - {1}",jsFile.Name,ex.Message);
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

                Parallel.ForEach(jsFile.NPMRegistries, (reg) =>
                {
                    var result = TryHttpGetString(Params.NPMRegistryGetURL.Replace("[NAME]", reg.Name), 5, true);

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
                        Console.WriteLine("Informações do registro [{0}] obtidas do repositório", reg.Name);
                    }
                });

                var list = jsFile.NPMRegistries.OrderBy(x => x.Packages.Count);

                Stopwatch stopwatch = new();
                foreach (var reg in list)
                {
                    stopwatch.Restart();
                    int numPackages = reg.Packages.Count;
                    int profDictionaryCap = (int)(jsFile.Lenght / SimilarityHandler.DEFAULT_K);

                    var client = new HttpClient();

                    Parallel.ForEach(reg.Packages, () => new JSCheckBufferBag(profDictionaryCap), (package, state,sPair) =>
                    {
                        try
                        {
                            using (var httpStream = client.GetStreamAsync(package.TarballUrl).Result)
                            {
                                httpStream.CopyTo(sPair.DownloadStream);
                            }
                            GetTarEntries(sPair, package, jsFile);
                            sPair.Clear();

                            if (package.BestSimilarity == 1.0)
                            {
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
                    client.Dispose();

                    reg.CountNumFilesChecked();
                    double hPackSimilarity = -1;
                    if (reg.HasAnyMatchedPackages())
                    {
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
                    var time = stopwatch.ElapsedMilliseconds / 1000.0;

                    Console.WriteLine("[{0}] - ({1}) - {2} arquivos de {3} versões em {4:0.00}s",
                        jsFile.Name,
                        reg.Name,
                        reg.TotalNumFilesOpened,
                        numPackages,
                        time);

                    if (hPackSimilarity == 1.0)
                    {
                        break;
                    }
                }

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
            int filesOpened = 0;

            var tarStream = sPair.SetTarReading();

            while (tarStream.GetNextEntry() is TarEntry entry)
            {
                if (!entry.IsDirectory)
                {
                    filesChecked++;
                    if (IsJsFile(entry.Name))
                    {
                        double sizeRatio = (double)entry.Size / jsFile.Lenght;

                        if (sizeRatio < 1.15 && sizeRatio > 0.85)
                        {
                            filesOpened++;
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

            package.FilesChecked = filesChecked;
            package.FilesOpened = filesOpened;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //Implementação manual de string.EndsWith(".js") para melhorias de desempenho
        private static bool IsJsFile(string fileName)
        {
            if (fileName.Length >= 3)
            {
                if (fileName[^1] == 's')
                {
                    if (fileName[^2] == 'j')
                    {
                        if (fileName[^3] == '.')
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static string TryHttpGetString(string url, int numrRetries, bool throwIfFail)
        {
            bool sucessful = false;
            Exception lastException = new();

            string result = string.Empty;

            for (int i = 0; i < numrRetries; i++)
            {
                try
                {
                    result = _httpClient.GetStringAsync(url).Result;
                    sucessful = true;
                }
                catch (Exception e)
                {
                    lastException = e;
                    Console.WriteLine("Falha ao tentar acessar ({0}), tentando novamente...", url);
                }
                if (sucessful)
                {
                    return result;
                }
            }
            if(!sucessful)
            {
                if(throwIfFail)
                {
                    throw lastException;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return result;
            }
        }
    }
}