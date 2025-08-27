using Client.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers
{
    /// <summary>
    /// Base controller providing common functionality for all controllers
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected readonly IUserSessionService UserSessionService;
        protected readonly ILogger Logger;

        protected BaseController(IUserSessionService userSessionService, ILogger logger)
        {
            UserSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get current user ID
        /// </summary>
        protected string? CurrentUserId => UserSessionService.GetUserId();

        /// <summary>
        /// Get current user name
        /// </summary>
        protected string? CurrentUserName => UserSessionService.GetUserName();

        /// <summary>
        /// Get current user email
        /// </summary>
        protected string? CurrentUserEmail => UserSessionService.GetUserEmail();

        /// <summary>
        /// Check if current user is authenticated
        /// </summary>
        protected bool IsAuthenticated => UserSessionService.IsAuthenticated();

        /// <summary>
        /// Get JWT token for current user
        /// </summary>
        protected string? JwtToken => UserSessionService.GetJwtToken();

        /// <summary>
        /// Set view bag with user information
        /// </summary>
        protected void SetUserViewBag()
        {
            ViewBag.IsLoggedIn = IsAuthenticated;
            ViewBag.UserName = CurrentUserName;
            ViewBag.UserEmail = CurrentUserEmail;
            ViewBag.UserId = CurrentUserId;
        }
    }
} 