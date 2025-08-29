using Client.MVC.Services;
using Client.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IUserSessionService UserSessionService;
        protected readonly ILogger Logger;
        protected readonly IErrorHandlingService ErrorHandlingService;
        protected readonly ICacheService CacheService;
        protected readonly IAntiForgeryService AntiForgeryService;

        protected BaseController(
            IUserSessionService userSessionService, 
            ILogger logger,
            IErrorHandlingService errorHandlingService,
            ICacheService cacheService,
            IAntiForgeryService antiForgeryService)
        {
            UserSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ErrorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
            CacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            AntiForgeryService = antiForgeryService ?? throw new ArgumentNullException(nameof(antiForgeryService));
        }

        protected string? CurrentUserId => UserSessionService.GetUserId();

        protected string? CurrentUserName => UserSessionService.GetUserName();

        protected string? CurrentUserEmail => UserSessionService.GetUserEmail();

        protected bool IsAuthenticated => UserSessionService.IsAuthenticated();

        protected string? JwtToken => UserSessionService.GetJwtToken();

        protected void SetUserViewBag()
        {
            ViewBag.IsLoggedIn = IsAuthenticated;
            ViewBag.UserName = CurrentUserName;
            ViewBag.UserEmail = CurrentUserEmail;
            ViewBag.UserId = CurrentUserId;
            
            // Add Anti-Forgery token for AJAX requests
            ViewBag.AntiForgeryToken = AntiForgeryService.GetToken();
            ViewBag.AntiForgeryHeaderName = AntiForgeryService.GetTokenHeaderName();
        }

        protected async Task<IActionResult> ExecuteWithErrorHandlingAsync(Func<Task<IActionResult>> action, string context = "")
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                await ErrorHandlingService.LogErrorAsync(ex, context);
                
                // Return appropriate error response based on exception type
                if (ex is HttpRequestException)
                {
                    return View("Error", new ErrorViewModel 
                    { 
                        Message = "خطا در ارتباط با سرور. لطفاً دوباره تلاش کنید.",
                        RequestId = HttpContext.TraceIdentifier 
                    });
                }
                
                return View("Error", new ErrorViewModel 
                { 
                    Message = "خطای غیرمنتظره رخ داده است.",
                    RequestId = HttpContext.TraceIdentifier 
                });
            }
        }

        protected async Task<T> GetCachedOrExecuteAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            return await CacheService.GetOrSetAsync(cacheKey, factory, expiration);
        }

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

            await ErrorHandlingService.LogInformationAsync($"User Action: {action}", logData);
        }

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

        protected void AddModelError(string key, string message)
        {
            ModelState.AddModelError(key, ErrorHandlingService.SanitizeErrorMessage(message));
        }

        protected bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        protected IActionResult JsonError(string message, object? additionalData = null)
        {
            return Json(new { success = false, message, data = additionalData });
        }

        protected IActionResult JsonSuccess(object? data = null, string message = "عملیات با موفقیت انجام شد")
        {
            return Json(new { success = true, message, data });
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