using AnalysisWebApp.Models;
using Common.ClassesDB;
using Common.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Common.ClassesLambda;
using Res;

namespace AnalysisWebApp.Controllers
{
    public class AnalysisController : Controller
    {

        [HttpGet("Analysis/Item/{id}")]
        public IActionResult Item(string id)
        {
            var model = new AnalysisViewModel();

            var tasks = new Task<bool>[5];

            tasks[0] = new Task<bool>(() =>
            {
                model.ParseExtensionInfo(DynamoDBHandler.GetEntry<ExtensionInfoResult>(DBTables.ExtensionInfo, id));
                return true;
            });
            tasks[1] = new Task<bool>(() =>
            {
                model.ParseExtensionPermissions(DynamoDBHandler.GetEntry<ExtensionPermissionsResult>(DBTables.Permissions, id));
                return true;
            });
            tasks[2] = new Task<bool>(() =>
            {
                model.ParseExtensionUrls(DynamoDBHandler.GetEntry<ExtensionURLsResult>(DBTables.URLs, id));
                return true;
            });
            tasks[3] = new Task<bool>(() =>
            {
                model.ParseExtensionJSFiles(DynamoDBHandler.GetEntry<ExtensionJSResult>(DBTables.JSFiles, id));
                return true;
            });
            tasks[4] = new Task<bool>(() =>
            {
                model.ParseExtensionVTresult(DynamoDBHandler.GetEntry<ExtensionVTResult>(DBTables.VirusTotal, id));
                return true;
            });

            foreach (var task in tasks)
            {
                task.Start();
            }

            Task.WaitAll(tasks);

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(string extensionUrl)
        {
            if (string.IsNullOrEmpty(extensionUrl)) return View();
            else
            {
                if (ExtensionDownloadhandler.IsThisUrlExtension(extensionUrl))
                {
                    var requestJson = JsonSerializer.Serialize(extensionUrl);

                    var lambdaResponse = LambdaHandler.CallFunction(Lambda.ScrappingInfo, requestJson, false).Result;

                    using (var reader = new StreamReader(lambdaResponse.Payload))
                    {
                        var response = reader.ReadToEnd();
                        var payload = JsonSerializer.Deserialize<LambdaAnalysisPayload>(response);

                        if(payload is not null)
                        {
                            var payloadJs = JsonSerializer.Serialize(new JsQueryLambdaPayload(payload, 0));

                            LambdaHandler.CallFunction(Lambda.Permissions, response, true);
                            LambdaHandler.CallFunction(Lambda.JS_Query, payloadJs, true);
                            LambdaHandler.CallFunction(Lambda.URLs, response, true);
                            LambdaHandler.CallFunction(Lambda.VirusTotal, response, true);

                            return RedirectToAction("Item", new { id = payload.AnalysisId });
                        }
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(extensionUrl as object);
                }
            }
        }
    }
}
