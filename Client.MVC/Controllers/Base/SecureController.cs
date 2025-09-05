using Client.MVC.Services.Abstractions;
using Client.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers.Base
{
    /// <summary>
    /// Base controller for secure operations with CSRF protection
    /// Includes anti-forgery token management
    /// </summary>
    public abstract class SecureController : AuthenticatedController
    {
        protected readonly IAntiForgeryService AntiForgeryService;

        protected SecureController(
            ICurrentUser currentUser,
            IAntiForgeryService antiForgeryService,
            ILogger logger)
            : base(currentUser, logger)
        {
            AntiForgeryService = antiForgeryService ?? throw new ArgumentNullException(nameof(antiForgeryService));
        }

        /// <summary>
        /// Set user information and security tokens in ViewBag
        /// </summary>
        protected new void SetUserViewBag()
        {
            // Call parent method first
            base.SetUserViewBag();
            
            // Add security-related ViewBag items
            ViewBag.AntiForgeryToken = AntiForgeryService.GetToken();
            ViewBag.AntiForgeryHeaderName = AntiForgeryService.GetTokenHeaderName();
        }

        /// <summary>
        /// Validate Anti-Forgery token for AJAX requests
        /// </summary>
        protected bool ValidateAntiForgeryToken()
        {
            try
            {
                var token = Request.Headers[AntiForgeryService.GetTokenHeaderName()].FirstOrDefault();
                if (string.IsNullOrEmpty(token))
                {
                    Logger.LogWarning("Anti-Forgery token header is missing");
                    return false;
                }

                var isValid = AntiForgeryService.ValidateToken(token);
                if (!isValid)
                {
                    Logger.LogWarning("Anti-Forgery token validation failed");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating Anti-Forgery token");
                return false;
            }
        }

        /// <summary>
        /// Get Anti-Forgery token for AJAX requests
        /// </summary>
        protected string GetAntiForgeryToken()
        {
            return AntiForgeryService.GetToken();
        }

        /// <summary>
        /// Set Anti-Forgery token in response cookie
        /// </summary>
        protected void SetAntiForgeryTokenInCookie()
        {
            AntiForgeryService.SetTokenInCookie(HttpContext);
        }
    }
}