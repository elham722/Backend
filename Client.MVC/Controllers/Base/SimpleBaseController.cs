using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers.Base
{
    /// <summary>
    /// Simple base controller with minimal dependencies
    /// Follows Single Responsibility Principle
    /// </summary>
    public abstract class SimpleBaseController : Controller
    {
        protected readonly ILogger Logger;

        protected SimpleBaseController(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Helper method to check if request is AJAX
        /// </summary>
        protected bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        /// <summary>
        /// Helper method to return JSON error response
        /// </summary>
        protected IActionResult JsonError(string message, object? additionalData = null)
        {
            return Json(new { success = false, message, data = additionalData });
        }

        /// <summary>
        /// Helper method to return JSON success response
        /// </summary>
        protected IActionResult JsonSuccess(object? data = null, string message = "عملیات با موفقیت انجام شد")
        {
            return Json(new { success = true, message, data });
        }

        /// <summary>
        /// Safe redirect to local URL
        /// </summary>
        protected IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}