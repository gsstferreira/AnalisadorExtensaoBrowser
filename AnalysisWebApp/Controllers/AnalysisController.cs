using AnalysisWebApp.Models;
using Common.ClassesDB;
using Common.Res;
using Common.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace AnalysisWebApp.Controllers
{
    public class AnalysisController : Controller
    {
        public IActionResult Index(string id)
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
                    return View("yay" as object);
                }
                else
                {
                    return View("nay" as object);
                }
            }
        }
    }
}
