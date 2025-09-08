using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Areas.Admin.Controllers
{
    /// <summary>
    /// Home controller for Admin Area
    /// Redirects /Admin to Dashboard
    /// </summary>
    [Area("Admin")]
    public class HomeController : Controller
    {
        /// <summary>
        /// Redirects /Admin to Dashboard
        /// </summary>
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}