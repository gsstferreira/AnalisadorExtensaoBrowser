using AnalysisWebApp.Models;
using Common.ClassesDB;
using Common.Handlers;
using Microsoft.AspNetCore.Mvc;
using Res;

namespace AnalysisWebApp.Controllers
{
    public class ListController : Controller
    {
        public IActionResult Index()
        {
            var extInfo = DynamoDBHandler.GetEntries<ExtensionInfoResult>(DBTables.ExtensionInfo);
            var model = new AnalysisListViewModel(0, extInfo);

            return View(model);
        }
    }
}
