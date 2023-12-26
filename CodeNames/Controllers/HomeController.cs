using CodeNames.CodeNames.Core.Services.GridGenerator;
using CodeNames.Models;
using CodeNames.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CodeNames.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGridGenerator _gridGeneratorService;

        public HomeController(ILogger<HomeController> logger,
            IGridGenerator gridGeneratorService)
        {
            _logger = logger;
            _gridGeneratorService = gridGeneratorService;
        }

        public IActionResult Index()
        {
            _gridGeneratorService.Generate();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
