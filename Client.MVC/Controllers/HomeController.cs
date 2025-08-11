using Client.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Client.MVC.Controllers
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
            // Read session data
            var token = HttpContext.Session.Get("JWTToken");
            var userName = HttpContext.Session.Get("UserName");
            
            if (token != null && userName != null)
            {
                var tokenString = Encoding.UTF8.GetString(token);
                var userNameString = Encoding.UTF8.GetString(userName);
                
                ViewBag.IsLoggedIn = true;
                ViewBag.UserName = userNameString;
                ViewBag.Token = tokenString;
            }
            else
            {
                ViewBag.IsLoggedIn = false;
            }
            
            return View();
        }
        [Authorize]
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
