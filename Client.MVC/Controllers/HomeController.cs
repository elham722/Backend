using Client.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Client.MVC.Services.Abstractions;
using Client.MVC.Controllers.Base;

namespace Client.MVC.Controllers
{
    public class HomeController : AuthenticatedController
    {
        public HomeController(
            ICurrentUser currentUser,
            ILogger<HomeController> logger)
            : base(currentUser, logger)
        {
        }
        
        public IActionResult Index()
        {
            // Set user information in view bag
            SetUserViewBag();
            
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
