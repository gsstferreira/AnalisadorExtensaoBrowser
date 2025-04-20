using Common.Classes;
using Common.ClassesJSON;
using Common.ClassesWeb.VirusTotal;
using Common.JsonSourceGenerators;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Common.Handlers
{
    public class VirusTotalHandler
    {
        private static readonly long crx_size_threshold = 32 * 1024 * 1024;
        private static readonly HttpClient _httpCLient = new();

        public static void UploadFileToVT(BrowserExtension extension)
        {
            try
            {
                string uploadUrl = Res.Params.VirusTotalFileURL;
                string b64File = extension.GetCrxAsB64();
                Console.WriteLine("{0:0.00}", 1.0 * b64File.Length / crx_size_threshold);

                if (b64File.Length > crx_size_threshold)
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

                    var json = _httpCLient.Send(urlRequest).Content.ReadAsStringAsync().Result;
                    uploadUrl = (JsonSerializer.Deserialize(json, VTUploadSG.Default.VTUploadUrlJson) ?? new VTUploadUrlJson()).UploadUrl;
                }

                string fileName = "compressed.zip";
                var sBuilder = new StringBuilder();
                sBuilder.Append("data:application/x-zip-compressed;name=");
                sBuilder.Append(fileName);
                sBuilder.Append(";base64,");

                sBuilder.Append(b64File);

                var multipartBody = sBuilder.ToString();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(uploadUrl),
                    Headers =
                {
                    {"accept", "application/json"},
                    {"x-apikey", Res.Keys.virus_total_api_key},
                },
                    Content = new MultipartFormDataContent()
                {
                    new StringContent(multipartBody)
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

                var response = _httpCLient.Send(request).Content;
                var content = response.ReadAsStringAsync().Result;
                Console.WriteLine(content);

                var queue = JsonSerializer.Deserialize(content, VTQueueSG.Default.VTQueueJson) ?? new VTQueueJson();
                extension.VirusTotalAnalysisUrl = queue.Information.AnalysisLinks.AnalysisUrl;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                extension.VirusTotalAnalysisUrl = string.Empty;
            }
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
                var content = JsonSerializer.Deserialize(response, VTResultSG.Default.VTResultJson) ?? new VTResultJson();

                var vtResult = new VTResponse
                {
                    Date = DateTime.UnixEpoch.AddSeconds(content.ResultData.Attributes.DateCompletion),
                    Status = content.ResultData.Attributes.Status,
                    Statistics = content.ResultData.Attributes.ResultStats,
                    ScanResults = []
                };

                foreach (var engineResult in content.ResultData.Attributes.Results)
                {
                    var value = engineResult.Value;
                    DateTime date;
                    if (long.TryParse(value.EngineUpdate, out long parsed))
                    {
                        int year = (int)parsed / 10000;
                        int month = (int)(parsed % 10000) / 100;
                        int day = (int)parsed % 100;

                        date = new DateTime(year, month, day);
                    }
                    else date = DateTime.MinValue;

                    var result = new AntivirusScanResult
                    {
                        Method = value.Method,
                        EngineVersion = value.EngineVersion,
                        EngineName = value.EngineName,
                        Category = value.Category,
                        EngineUpdate = date,
                    };

                    vtResult.ScanResults.Add(result);
                }
                return vtResult;
            }
        }
    }
}
