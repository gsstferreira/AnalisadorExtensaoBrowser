using AnalysisWebApp.Models;
using Microsoft.AspNetCore.Mvc;

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
            return RedirectToAction("Index","List");
        }
        public IActionResult Error()
        {
            if(TempData["ThrownException"] is string exceptionJson)
            {
                return View(ExceptionViewModel.FromJson(exceptionJson));
            }

            return View(new ExceptionViewModel());
        }
    }
}
