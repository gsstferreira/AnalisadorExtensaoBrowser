using AnalysisWebApp.Models;
using Common.ClassesDB;
using Common.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace AnalysisWebApp.Controllers
{
    public class AnalysisListController : Controller
    {
        public IActionResult Index()
        {
            var extInfo = DynamoDBHandler.GetEntries<ExtensionInfoResult>(Common.Res.DBTables.ExtensionInfo, 0, 0);
            var model = new AnalysisListViewModel(0, extInfo);

            return View(model);
        }
    }
}
