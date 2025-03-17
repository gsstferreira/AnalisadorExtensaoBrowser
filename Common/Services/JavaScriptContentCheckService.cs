using Common.Classes;
using Common.ClassesWeb.NPMRegistry;
using F23.StringSimilarity;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace Common.Services
{
    public partial class JavaScriptContentCheckService
    {
        // client para chamadas HTTP
        internal static readonly HttpClient _httpClient = new();

        //Calcula a similaridade entre textos - https://en.wikipedia.org/wiki/Cosine_similarity
        internal static readonly Cosine _cosine = new(5);

        //Calcula a similaridade entre duas strings - https://en.wikipedia.org/wiki/Levenshtein_distance
        internal static readonly Levenshtein _levDistance = new();

        public static void CheckJSFiles(BrowserExtension extension)
        {
            GetJSFiles(extension);
            var numFiles = extension.ContainedJSFiles.Count;
            Console.WriteLine(string.Format("{0} arquivos JavaScript encontrados na extensão", numFiles));

            var tasks = new Task[numFiles];

            for (int i = 0; i < numFiles; i++)
            {
                var jsFile = extension.ContainedJSFiles[i];

                var task = new Task(() =>
                {
                    GetPotentialNPMPackages(jsFile);
                    if (jsFile.NPMRegistries.Count > 0)
                    {
                        ValidateNPMPackages(jsFile);
                    }
                    Console.WriteLine(jsFile.ToString());
                });
                tasks[i] = task;
                task.Start();
                Task.Delay(1000).Wait();
            }
            Task.WaitAll(tasks);
        }

        internal static void GetJSFiles(BrowserExtension extension)
        {
            foreach (var entry in extension.CrxArchive.Entries)
            {
                if (entry.Name.EndsWith(".js"))
                {
                    var jsFile = new JSFile(entry);
                    extension.ContainedJSFiles.Add(jsFile);
                }
            }
            extension.ContainedJSFiles = [.. extension.ContainedJSFiles.OrderBy(f => f.Name)];
        }
        internal static void GetPotentialNPMPackages(JSFile jsFile)
        {
            try
            {
                var jsName = jsFile.Name.EndsWith(".min") ? jsFile.Name.Replace(".min", string.Empty) : jsFile.Name;

                var result = _httpClient.GetStringAsync(Res.Params.NPMRegistryQueryURL.Replace("[NAME]", jsName)).Result;

                if (!string.IsNullOrEmpty(result))
                {
                    var threadBag = new ConcurrentBag<NPMRegistry>();

                    var matches = JsonNode.Parse(result)["objects"].AsArray();

                    foreach (var match in matches)
                    {
                        var registry = CheckNPMPackage(match);

                        if (!string.IsNullOrEmpty(registry.Name))
                        {
                            if (jsName.Contains(registry.Name) || registry.Name.Contains(jsName))
                            {
                                jsFile.NPMRegistries.Add(registry);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("exception: " + ex.Message);
            }
        }

        internal static void ValidateNPMPackages(JSFile jsFile)
        {

            Parallel.ForEach(jsFile.NPMRegistries, registry =>
            {
                var result = _httpClient.GetStringAsync(Res.Params.NPMRegistryGetURL.Replace("[NAME]", registry.Name)).Result;
                if (!string.IsNullOrEmpty(result))
                {
                    var json = JsonNode.Parse(result);
                    var versions = json["versions"].AsObject();
                    var dates = json["time"].AsObject();

                    for (int i = 0; i < versions.Count; i++)
                    {
                        var package = versions.ElementAt(i);

                        var version = package.Value["version"].Deserialize<string>() ?? string.Empty;
                        DateTime date = dates[version].Deserialize<DateTime>();
                        var tarUrl = package.Value["dist"]["tarball"].Deserialize<string>() ?? string.Empty;

                        var npmPackage = new NPMPackage
                        {
                            Name = registry.Name,
                            Version = version,
                            ReleaseDate = date,
                            TarballUrl = tarUrl,
                            BestSimilarity = 0,
                            NumJsFiles = 0,
                        };
                        registry.Packages.Add(npmPackage);
                    }
                }
                registry.Packages = registry.Packages.OrderByDescending(p => p.ReleaseDate).ToList();
            });

            bool perfectMatchFound = false;

            foreach (var registry in jsFile.NPMRegistries)
            {
                if (perfectMatchFound)
                {
                    break;
                }

                double highestSimilarity = 0;

                Parallel.ForEach(registry.Packages, (package, state) =>
                {
                    try
                    {
                        var localPerfectMatch = false;

                        using (var downloadStream = new MemoryStream())
                        {
                            using (var httpStream = _httpClient.GetStreamAsync(package.TarballUrl).Result)
                            {
                                httpStream.CopyTo(downloadStream);
                            }
                            downloadStream.Seek(0, SeekOrigin.Begin);
                            byte[] buffer = new byte[2];
                            downloadStream.Read(buffer, 0, buffer.Length);
                            downloadStream.Seek(0, SeekOrigin.Begin);

                            if (buffer[0] == 0x1F && buffer[1] == 0x8B)
                            {
                                using var tgz = new GZipInputStream(downloadStream);
                                localPerfectMatch = GetTarEntries(tgz, package, jsFile);
                            }
                            else
                            {
                                localPerfectMatch = GetTarEntries(downloadStream, package, jsFile);
                            }
                        }
                        if (localPerfectMatch)
                        {
                            perfectMatchFound = true;
                            state.Break();
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(registry.Name + " | " + ex.Message + " | " + jsFile.Name);
                    }
                });

                foreach (var pack in registry.Packages)
                {
                    jsFile.TotalFilesChecked += pack.NumJsFiles;
                }

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
                    registry.Packages.Clear();
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
            jsFile.NPMRegistries.Clear();
        }

        internal static NPMRegistry CheckNPMPackage(JsonNode json)
        {
            try
            {
                var package = json["package"];
                var links = package["links"];
                return new NPMRegistry
                {
                    Name = package["name"].Deserialize<string>() ?? string.Empty,
                    HomepageUrl = links["homepage"].Deserialize<string>() ?? string.Empty,
                    RepositoryUrl = links["repository"].Deserialize<string>() ?? string.Empty,
                    BugTrackerUrl = links["bugs"].Deserialize<string>() ?? string.Empty,
                    NPMPageUrl = links["npm"].Deserialize<string>() ?? string.Empty,
                };

            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Message);
                return new NPMRegistry();
            }
        }
        internal static bool GetTarEntries(Stream stream, NPMPackage package, JSFile jsFile)
        {
            using var tarInputStream = new TarInputStream(stream, Encoding.Default);
            using var memStream = new MemoryStream();
            using var sReader = new StreamReader(memStream);
            while (tarInputStream.GetNextEntry() is TarEntry entry)
            {
                if (entry.Name.EndsWith(".js"))
                {
                    package.NumJsFiles += 1;
                    double sizeRatio = (double)entry.Size / (double)jsFile.Size;

                    if (sizeRatio < 1.15 && sizeRatio > 0.85)
                    {
                        memStream.SetLength(0);
                        tarInputStream.CopyEntryContents(memStream);
                        memStream.Seek(0, SeekOrigin.Begin);
                        var content = sReader.ReadToEnd();

                        var similarity = _cosine.Similarity(jsFile.Content, content);

                        if (similarity > 0.9 && similarity > package.BestSimilarity)
                        {
                            package.BestSimilarity = similarity;

                            if (similarity > JSFile.PerfectMatchThreshold)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}