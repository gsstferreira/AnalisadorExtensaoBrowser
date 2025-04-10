using Common.Classes;
using Common.ClassesWeb.GoogleSafeBrowsing;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Common.Services
{
    public class UrlCheckService
    {
        private static readonly HttpClient _httpClient = new();
        public static void CheckURLs(BrowserExtension extension)
        {
            FindURLs(extension);

            //extension.ContainedURLs.Add(new Url("https://testsafebrowsing.appspot.com/s/unwanted.html"));
            //extension.ContainedURLs.Add(new Url("https://testsafebrowsing.appspot.com/s/phishing.html"));
            //extension.ContainedURLs.Add(new Url("https://testsafebrowsing.appspot.com/s/malware.html"));

            CheckURLsSafety(extension);
        }
        private static void FindURLs(BrowserExtension extension)
        {
            foreach (var entry in extension.CrxArchive.Entries)
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
        }
        private static void CheckURLsSafety(BrowserExtension extension)
        {
            var sb = new StringBuilder();

            foreach (var url in extension.ContainedURLs)
            {
                sb.Append(string.Format("{{\"url\":\"{0}\"}},", url.Path));
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

            using(var reader = new StreamReader(response.Content.ReadAsStream(), Encoding.UTF8))
            {
                text = reader.ReadToEnd();
            }
            var threats = JsonSerializer.Deserialize<GSBResponse>(text) ?? new GSBResponse();

            foreach (var url in extension.ContainedURLs)
            {
                url.ThreatType = GSBThreatType.SAFE;
                foreach (var threat in threats.ThreatMatches)
                {
                    if (threat.ThreatEntry.url.Equals(url.Path))
                    {
                        url.ThreatType = threat.ThreatType;
                        break;
                    }
                }
            }
            extension.ContainedURLs = [.. extension.ContainedURLs.OrderBy(u => u.Path)];
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
    }
}
