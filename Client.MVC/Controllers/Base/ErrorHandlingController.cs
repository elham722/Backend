using Client.MVC.Services.Abstractions;
using Client.MVC.Services;
using Client.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers.Base
{
    /// <summary>
    /// Base controller with error handling and caching capabilities
    /// Follows Decorator pattern for error handling
    /// </summary>
    public abstract class ErrorHandlingController : SecureController
    {
        protected readonly IErrorHandlingService ErrorHandlingService;
        protected readonly ICacheService CacheService;

        protected ErrorHandlingController(
            ICurrentUser currentUser,
            IAntiForgeryService antiForgeryService,
            IErrorHandlingService errorHandlingService,
            ICacheService cacheService,
            ILogger logger)
            : base(currentUser, antiForgeryService, logger)
        {
            ErrorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
            CacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        /// <summary>
        /// Execute action with comprehensive error handling
        /// </summary>
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

        /// <summary>
        /// Execute action with caching support
        /// </summary>
        protected async Task<T> GetCachedOrExecuteAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            return await CacheService.GetOrSetAsync(cacheKey, factory, expiration);
        }

        /// <summary>
        /// Add model error with sanitization
        /// </summary>
        protected void AddModelError(string key, string message)
        {
            ModelState.AddModelError(key, ErrorHandlingService.SanitizeErrorMessage(message));
        }

        /// <summary>
        /// Execute action with error handling and return JSON response
        /// </summary>
        protected async Task<IActionResult> ExecuteWithJsonErrorHandlingAsync(Func<Task<IActionResult>> action, string context = "")
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                await ErrorHandlingService.LogErrorAsync(ex, context);
                
                if (IsAjaxRequest())
                {
                    return JsonError("خطا در انجام عملیات. لطفاً دوباره تلاش کنید.");
                }
                
                return View("Error", new ErrorViewModel 
                { 
                    Message = "خطای غیرمنتظره رخ داده است.",
                    RequestId = HttpContext.TraceIdentifier 
                });
            }
        }
    }
}