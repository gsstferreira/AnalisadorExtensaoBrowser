using Common.Classes;
using Common.ClassesJSON;
using Common.ClassesWeb.VirusTotal;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Common.Handlers
{
    public class VirusTotalHandler
    {
        private static readonly long crx_size_threshold = 32 * 1024 * 1024;
        private static readonly HttpClient _httpCLient = new();

        public static void UploadFileToVT(BrowserExtension extension)
        {
            string uploadUrl = Res.Params.VirusTotalFileURL;

            if (extension.GetCrxSize() > crx_size_threshold) 
            {
                var urlRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Res.Params.VirusTotalUrlRequest),
                    Headers =
                    {
                        {"accept", "application/json"},
                        {"x-apikey", Res.Keys.virus_total_api_key},
                    },
                };

                var urlJson = _httpCLient.Send(urlRequest).Content.ReadAsStringAsync().Result;
                uploadUrl = (JsonSerializer.Deserialize<VTUploadUrlJson>(urlJson) ?? new VTUploadUrlJson()).UploadUrl;
            }

            string fileName = "ext_compressed.zip";
            var sBuilder = new StringBuilder();
            sBuilder.Append("data:application/x-zip-compressed;name=");
            sBuilder.Append(fileName);
            sBuilder.Append(";base64,");
            sBuilder.Append(extension.GetCrxAsB64());

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uploadUrl),
                Headers =
                {
                    {"accept", "application/json"},
                    {"x-apikey", Res.Keys.virus_total_api_key},
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
            var content = JsonSerializer.Deserialize<VTQueueJson>(response) ?? new VTQueueJson();

            extension.VirusTotalAnalysisUrl = content.Information.AnalysisLinks.AnalysisUrl;
        }
        public static VTResponse CheckVTAnalysis(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("No analysis link found!");
                return new VTResponse();
            }
            else
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                    Headers =
                    {
                        {"accept", "application/json" },
                        {"x-apikey", Res.Keys.virus_total_api_key }
                    }
                };

                var response = _httpCLient.Send(request).Content.ReadAsStringAsync().Result;
                var content = JsonSerializer.Deserialize<VTResultJson>(response) ?? new VTResultJson();

                var vtResult = new VTResponse 
                {
                    Date = DateTime.UnixEpoch.AddSeconds(content.ResultData.Attributes.DateCompletion),
                    Status = content.ResultData.Attributes.Status,
                    Statistics = content.ResultData.Attributes.ResultStats,
                    ScanResults = []
                };

                foreach(var engineResult in content.ResultData.Attributes.Results)
                {
                    var value = engineResult.Value;

                    var rawDate = long.TryParse(value.EngineUpdate, out long parsed);
                    int year = (int)parsed / 10000;
                    int month = (int)(parsed % 10000) / 100;
                    int day = (int)parsed % 100;

                    var result = new AntivirusScanResult 
                    { 
                        Method = value.Method,
                        EngineVersion = value.EngineVersion,
                        EngineName = value.EngineName,
                        Category = value.Category,
                        EngineUpdate = new DateTime(year, month, day),
                    };

                    vtResult.ScanResults.Add(result);
                }
                return vtResult;
            }
        }
    }
}
