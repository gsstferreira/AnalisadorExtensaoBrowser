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
            try
            {
                var extInfo = DynamoDBHandler.GetEntries<ExtInfoResult>(DBTables.ExtensionInfo);
                var model = new AnalysisListViewModel(0, extInfo);

                return View(model);
            }
            catch (Exception ex) 
            {
                var exception = new ExceptionViewModel(ex);
                TempData["ThrownException"] = exception.AsJson();
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
