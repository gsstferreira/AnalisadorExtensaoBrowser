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
            return RedirectToAction("List","AnalysisList");
        }
    }
}
