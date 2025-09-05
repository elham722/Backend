using Client.MVC.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers.Base
{
    /// <summary>
    /// Base controller for authenticated operations
    /// Provides current user information
    /// </summary>
    public abstract class AuthenticatedController : SimpleBaseController
    {
        protected readonly ICurrentUser CurrentUser;

        protected AuthenticatedController(
            ICurrentUser currentUser,
            ILogger logger)
            : base(logger)
        {
            CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        /// <summary>
        /// Get current user ID
        /// </summary>
        protected string? CurrentUserId => CurrentUser.GetUserId();

        /// <summary>
        /// Get current user name
        /// </summary>
        protected string? CurrentUserName => CurrentUser.GetUserName();

        /// <summary>
        /// Get current user email
        /// </summary>
        protected string? CurrentUserEmail => CurrentUser.GetUserEmail();

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        protected bool IsAuthenticated => CurrentUser.IsAuthenticated();

        /// <summary>
        /// Get current user roles
        /// </summary>
        protected IEnumerable<string> CurrentUserRoles => CurrentUser.GetUserRoles();

        /// <summary>
        /// Set user information in ViewBag for views
        /// </summary>
        protected void SetUserViewBag()
        {
            ViewBag.IsLoggedIn = IsAuthenticated;
            ViewBag.UserName = CurrentUserName;
            ViewBag.UserEmail = CurrentUserEmail;
            ViewBag.UserId = CurrentUserId;
            ViewBag.UserRoles = CurrentUserRoles.ToList();
        }

        /// <summary>
        /// Log user action for audit purposes
        /// </summary>
        protected async Task LogUserActionAsync(string action, object? additionalData = null)
        {
            var logData = new
            {
                UserId = CurrentUserId,
                UserName = CurrentUserName,
                Action = action,
                Controller = ControllerContext.ActionDescriptor.ControllerName,
                ActionName = ControllerContext.ActionDescriptor.ActionName,
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                AdditionalData = additionalData
            };

            Logger.LogInformation("User Action: {Action} by User: {UserId}", action, CurrentUserId);
        }
    }
}