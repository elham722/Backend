using Client.MVC.Controllers.Base;
using Client.MVC.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Areas.Admin.Controllers
{
    /// <summary>
    /// Base controller for all Admin area controllers
    /// Provides common functionality and authorization for admin operations
    /// </summary>
   
    public abstract class AdminBaseController : SecureController
    {
        protected AdminBaseController(
            ICurrentUser currentUser,
            IAntiForgeryService antiForgeryService,
            ILogger logger) 
            : base(currentUser, antiForgeryService, logger)
        {
        }

        /// <summary>
        /// Gets the current admin user's ID
        /// </summary>
        protected string CurrentAdminId => CurrentUser.GetUserId() ?? string.Empty;

        /// <summary>
        /// Gets the current admin user's username
        /// </summary>
        protected string CurrentAdminUsername => CurrentUser.GetUserName() ?? string.Empty;

        /// <summary>
        /// Checks if the current user has SuperAdmin role
        /// </summary>
        protected bool IsSuperAdmin => CurrentUser.GetUserRoles()?.Contains("SuperAdmin") == true;

        /// <summary>
        /// Sets admin-specific view data
        /// </summary>
        protected void SetAdminViewData()
        {
            ViewData["AdminUsername"] = CurrentAdminUsername;
            ViewData["IsSuperAdmin"] = IsSuperAdmin;
            ViewData["AdminArea"] = true;
        }

        /// <summary>
        /// Returns a standardized admin error view
        /// </summary>
        protected IActionResult AdminError(string message, string? details = null)
        {
            ViewData["ErrorMessage"] = message;
            ViewData["ErrorDetails"] = details;
            return View("Error");
        }

        /// <summary>
        /// Returns a standardized admin success view
        /// </summary>
        protected IActionResult AdminSuccess(string message, string? returnUrl = null)
        {
            ViewData["SuccessMessage"] = message;
            ViewData["ReturnUrl"] = returnUrl;
            return View("Success");
        }
    }
}