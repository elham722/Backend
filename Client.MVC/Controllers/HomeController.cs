using Client.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Client.MVC.Services;
using Microsoft.AspNetCore.Authorization;

namespace Client.MVC.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IUserSessionService userSessionService, ILogger<HomeController> logger, IErrorHandlingService errorHandlingService, ICacheService cacheService, IAntiForgeryService antiForgeryService)
            : base(userSessionService, logger, errorHandlingService, cacheService, antiForgeryService)
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
