using Common.Classes;
using Common.ClassesWeb.VirusTotal;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Common.Services
{
    public class VirusTotalService
    {
        internal static readonly HttpClient _httpCLient = new();

        public static void UploadFileToVT(BrowserExtension extension)
        {
            string fileName = "ext_compressed.zip";
            var sBuilder = new StringBuilder();
            sBuilder.Append("data:application/x-zip-compressed;name=");
            sBuilder.Append(fileName);
            sBuilder.Append(";base64,");
            sBuilder.Append(extension.CrxB64);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Res.Params.VirusTotalFileURL),
                Headers =
                {
                    {"accept", "application/json" },
                    { "x-apikey", Res.Keys.virus_total_api_key },
                },
                Content = new MultipartFormDataContent
                {
                    new StringContent(sBuilder.ToString())
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/x-zip-compressed"),
                            ContentDisposition = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = "file",
                                FileName = fileName,
                            }
                        }
                    }
                }
            };

            var response = _httpCLient.Send(request).Content.ReadAsStringAsync().Result;
            var json = JsonNode.Parse(response)["data"];
            extension.VirusTotalAnalysisUrl = json["links"]["self"].GetValue<string>();
        }
        public static void CheckVTAnalysis(BrowserExtension extension)
        {
            if (string.IsNullOrEmpty(extension.VirusTotalAnalysisUrl))
            {
                Console.WriteLine("No analysis link found!");
            }
            else
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(extension.VirusTotalAnalysisUrl),
                    Headers =
                    {
                        {"accept", "application/json" },
                        {"x-apikey", Res.Keys.virus_total_api_key }
                    }
                };

                var response = _httpCLient.Send(request);
                var content = response.Content.ReadAsStringAsync().Result;

                var json = JsonObject.Parse(content)["data"]["attributes"].AsObject();

                var vtResult = new VTResponse 
                {
                    Date = DateTime.UnixEpoch.AddSeconds(json["date"].GetValue<long>()),
                    Status = json["status"].GetValue<string>(),
                    Statistics = json["stats"].Deserialize<VTResponseStatistics>(),
                    ScanResults = []
                };

                var avResults = json["results"].AsObject();

                foreach(var att in avResults)
                {
                    var result = att.Value.Deserialize<AntivirusScanResult>() ?? new AntivirusScanResult();

                    var rawDate = int.Parse(att.Value["engine_update"].GetValue<string>());
                    var year = rawDate / 10000;
                    var month = (rawDate % 10000)/100;
                    var day = rawDate % 100;

                    result.EngineUpdate = new DateTime(year,month,day);

                    vtResult.ScanResults.Add(result);
                }
                extension.VirusTotalResult = vtResult;
            }
        }
    }
}
