using AnalysisWebApp.Models;
using Common.ClassesDB;
using Common.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Common.ClassesLambda;
using Res;
using System.Threading.Tasks;

namespace AnalysisWebApp.Controllers
{
    public class AnalysisController : Controller
    {
        [HttpGet("Analysis/Item/{id}")]
        public IActionResult Item(string id)
        {
            try 
            {
                var model = new AnalysisViewModel();

                var tFactory = Task.Factory;

                var tasks = new Task[5]
                {
                    tFactory.StartNew(() => model.ParseInfo(DynamoDBHandler.GetEntry<ExtInfoResult>(DBTables.ExtensionInfo, id))),
                    tFactory.StartNew(() => model.ParsePermissions(DynamoDBHandler.GetEntry<ExtPermissionsResult>(DBTables.Permissions, id))),
                    tFactory.StartNew(() => model.ParseUrls(DynamoDBHandler.GetEntry<ExtURLsResult>(DBTables.URLs, id))),
                    tFactory.StartNew(() => model.ParseJSFiles(DynamoDBHandler.GetEntry<ExtJSResult>(DBTables.JSFiles, id))),
                    tFactory.StartNew(() => model.ParseVirusTotal(DynamoDBHandler.GetEntry<ExtVTResult>(DBTables.VirusTotal, id)))
                };

                Task.WaitAll(tasks);

                return View(model);
            }
            catch (Exception ex)
            {
                var exception = new ExceptionViewModel(ex);
                TempData["ThrownException"] = exception.AsJson();
                return RedirectToAction("Error", "Home");
            }
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
                // Verificação se a URL é uma extensão Chromium específica da Chrome Web Store
                if (ExtensionDownloadhandler.IsThisUrlExtension(extensionUrl))
                {
                    var requestJson = JsonSerializer.Serialize(extensionUrl);

                    // Invocando a função Lambda para o 'Método 1'
                    // Argumento 'false' na chamada indica o tipo "RequestResponse"
                    // Invocações tipo "RequestResponse" devem retornar uma resposta ao requisitante 
                    var lambdaResponse = LambdaHandler.CallFunction(Lambda.ScrappingInfo, requestJson, false).Result;

                    using (var reader = new StreamReader(lambdaResponse.Payload))
                    {
                        var response = reader.ReadToEnd();
                        var payload = JsonSerializer.Deserialize<LambdaAnalysisPayload>(response) ?? default;

                        // Confirmação que o payload foi lido corretamente
                        if(payload is not null)
                        {
                            var payloadJs = JsonSerializer.Serialize(new JsQueryLambdaPayload(payload, 0));
                            // Invocação das outras funções Lambda
                            // Argumento 'true' na chamada indica o tipo "Event"
                            // Invocações tipo "Event" não precisam retornar uma resposta ao requisitante
                            LambdaHandler.CallFunction(Lambda.Permissions, response, true);
                            LambdaHandler.CallFunction(Lambda.URLs, response, true);
                            LambdaHandler.CallFunction(Lambda.JS_Query, payloadJs, true);
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
