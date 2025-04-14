using AnalysisWebApp.Models;
using Common.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AnalysisWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            var extension = ExtensionDownloadhandler.GetExtension(Constants.UrlTest, Common.Enums.ExtDownloadType.Full);

            var extViewModel = new ExtensionViewModel(extension);

            return View(extViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
