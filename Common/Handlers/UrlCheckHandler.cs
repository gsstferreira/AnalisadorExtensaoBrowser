using Common.Classes;
using Common.ClassesWeb.GoogleSafeBrowsing;
using Common.Enums;
using Common.JsonSourceGenerators;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Common.Handlers
{
    public class UrlCheckHandler
    {
        private static readonly HttpClient _httpClient = new();
        private static readonly HttpClient _pingHttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(4)
        };

        private const int _gsb_batch_size = 500;
        public static void CheckURLs(BrowserExtension extension)
        {
            Console.WriteLine("Buscando string em formato de URLs na extensão...");
            var urls = FindURLs(extension);
            Console.WriteLine("Consultando base de URLs do Google Safe Browsing...");
            urls = CheckURLsSafety(urls);
            Console.WriteLine("Removendo URLs inválidas...");
            extension.ContainedURLs = FilterInvalidUrls(urls);
            Console.WriteLine("Análise de URLs concluída.");
        }
        private static List<Url> FindURLs(BrowserExtension extension)
        {
            foreach (var entry in extension.ExtensionContent.Entries)
            {
                var name = entry.Name;

                //Lendo arquivos que podem conter URLs
                if (CanContainURLs(name))
                {
                    var reader = new StreamReader(entry.Open());
                    var text = reader.ReadToEnd();
                    reader.Close();

                    var matches = Regex.Matches(text, Res.SearchStrs.regex_URL);

                    extension.ContainedURLs.AddRange(from Match match in matches
                                                     let url = new Url(match.Value
                                                         .Trim()
                                                         .Replace("\"", string.Empty)
                                                         .Replace("'", string.Empty))
                                                     where !extension.ContainedURLs.Contains(url)
                                                     select url);
                    var matches_ip = Regex.Matches(text, Res.SearchStrs.regex_IP);
                    foreach (Match match in matches_ip)
                    {
                        var numbers = match.Value.Split('.');
                        bool isValid = true;

                        foreach (var number in numbers)
                        {
                            int num = int.Parse(number);
                            if (num > 255)
                            {
                                isValid = false;
                                break;
                            }
                        }

                        var url = new Url(match.Value.Trim());

                        if (isValid && !extension.ContainedURLs.Contains(url))
                        {
                            extension.ContainedURLs.Add(url);
                        }
                    }
                }
            }
            return [.. extension.ContainedURLs.DistinctBy(x => x.OriginalUrl)];
        }
        private static List<Url> CheckURLsSafety(List<Url> urls)
        {
            var urlBag = new ConcurrentBag<Url>();

            int urlCount = urls.Count;
            var calls = new List<Task>();

            do
            {
                var batch = urls.Take(_gsb_batch_size);
                urls = [.. urls.Skip(_gsb_batch_size)];
                urlCount -= _gsb_batch_size;

                calls.Add(Task.Factory.StartNew(() => 
                {
                    var sb = new StringBuilder();
                    foreach (var url in batch)
                    {
                        sb.Append(string.Format("{{\"url\":\"{0}\"}},", url.OriginalUrl));
                    }

                    var body = Res.Jsons.GSBLookupRequest
                        .Trim()
                        .Replace("[URLS]", sb.ToString().Trim());

                    var request = new HttpRequestMessage(HttpMethod.Post, Res.Params.GSBLookupURL + Res.Keys.google_api_key)
                    {
                        Content = new StringContent(body)
                    };

                    var response = _httpClient.Send(request);

                    string text = string.Empty;

                    using (var reader = new StreamReader(response.Content.ReadAsStream(), Encoding.UTF8))
                    {
                        text = reader.ReadToEnd();
                    }
                    var threats = JsonSerializer.Deserialize(text, GSBResponseSG.Default.GSBResponse) ?? new GSBResponse();

                    foreach (var url in batch)
                    {
                        url.ThreatType = GSBThreatType.SAFE;
                        foreach (var threat in threats.ThreatMatches)
                        {
                            if (threat.ThreatEntry.Url.Equals(url.OriginalUrl))
                            {
                                url.ThreatType = threat.ThreatType;
                                break;
                            }
                        }
                        urlBag.Add(url);
                    }
                }));
            } while (urlCount > 0);

            Task.WaitAll([.. calls]);

            return [.. urlBag.OrderBy(u => u.ThreatType).ThenBy(u => u.OriginalUrl)];
        }
        private static bool CanContainURLs(string fileName)
        {
            if(!string.IsNullOrEmpty(fileName))
            {
                if(fileName.EndsWith(".html") || fileName.EndsWith(".htm"))
                {
                    return true;
                }
                else if (fileName.EndsWith(".css"))
                {
                    return true;
                }
                else if (fileName.EndsWith(".js"))
                {
                    return true;
                }
                else if (fileName.EndsWith(".json"))
                {
                    return true;
                }
                else if (fileName.EndsWith(".xml"))
                {
                    return true;
                }
                else if (fileName.EndsWith(".yml"))
                {
                    return true;
                }
                else if (fileName.EndsWith(".txt"))
                {
                    return true;
                }
            }

            return false;
        }
        private static List<Url> FilterInvalidUrls(ICollection<Url> urls)
        {
            var filterBag = new ConcurrentBag<Url>();
            var tasks = new List<Task<bool>>();

            Parallel.ForEach(urls, url =>
            {
                var headUrl = url.OriginalUrl;
                if (!headUrl.StartsWith("http"))
                {
                    headUrl = "http://" + headUrl;
                }

                var uri = new Uri(headUrl);
                bool dnsSolved = false;

                try
                {
                    var ipAddresses = Dns.GetHostAddresses(uri.Host);
                    if (ipAddresses.Length > 0)
                    {
                        Console.WriteLine("DNS SOLVED - " + headUrl);
                        dnsSolved = true;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("DNS UNSOLVED - " + headUrl);
                    dnsSolved = false;
                }

                if (dnsSolved)
                {
                    try
                    {

                        var response = _pingHttpClient.Send(new HttpRequestMessage(HttpMethod.Head, headUrl));
                        var requestMessage = response.RequestMessage;

                        if (requestMessage is not null)
                        {
                            var address = requestMessage.RequestUri;

                            if (address is not null)
                            {
                                filterBag.Add(new Url
                                {
                                    OriginalUrl = url.OriginalUrl,
                                    Host = address.Host,
                                    RedirectUrl = address.AbsoluteUri,
                                    IsHttps = address.AbsoluteUri.StartsWith("https"),
                                    ThreatType = url.ThreatType,
                                    Type = UrlType.PUBLIC,
                                });
                            }
                        }
                    }
                    catch (Exception)
                    {
                        var u = url.OriginalUrl;
                        if (Regex.Match(u, Res.SearchStrs.regex_IP).Success)
                        {
                            if (u.StartsWith("127"))
                            {
                                filterBag.Add(new Url
                                {
                                    OriginalUrl = u,
                                    Host = u,
                                    IsHttps = false,
                                    ThreatType = url.ThreatType,
                                    Type = UrlType.OWN_DEVICE,
                                });
                            }
                            else if (u.StartsWith("192.168"))
                            {
                                filterBag.Add(new Url
                                {
                                    OriginalUrl = u,
                                    Host = u,
                                    IsHttps = false,
                                    ThreatType = url.ThreatType,
                                    Type = UrlType.LOCAL,
                                });
                            }
                        }
                    }
                }
            });

            return [.. filterBag.OrderBy(u => u.OriginalUrl)];
        }
    }
}
