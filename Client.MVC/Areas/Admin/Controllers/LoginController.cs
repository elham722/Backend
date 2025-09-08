using System.ComponentModel.DataAnnotations;
using Client.MVC.Services.Abstractions;
using Client.MVC.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Client.MVC.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for Admin Area login
    /// Handles admin-specific login flow
    /// </summary>
    [Area("Admin")]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly IAuthApiClient _authApiClient;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            IAuthApiClient authApiClient,
            ICurrentUser currentUser,
            ILogger<LoginController> logger)
        {
            _authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the admin login page
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            // If user is already authenticated and has admin role, redirect to dashboard
            if (_currentUser.IsAuthenticated())
            {
                var userRoles = _currentUser.GetUserRoles();
                if (userRoles != null && (userRoles.Contains("Admin") || userRoles.Contains("SuperAdmin")))
                {
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            ViewData["Title"] = "ورود به پنل ادمین";
            return View();
        }

        /// <summary>
        /// Handles admin login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Index", request);
                }

                // Perform login
                var loginResult = await _authApiClient.LoginAsync(new Backend.Application.Features.UserManagement.DTOs.Auth.LoginRequest
                {
                    EmailOrUsername = request.Email,
                    Password = request.Password,
                    RememberMe = request.RememberMe
                });

                if (!loginResult.IsSuccess)
                {
                    ModelState.AddModelError("", loginResult.ErrorMessage ?? "خطا در ورود");
                    return View("Index", request);
                }

                // Check if user has admin role
                var userRoles = _currentUser.GetUserRoles();
                if (userRoles == null || (!userRoles.Contains("Admin") && !userRoles.Contains("SuperAdmin")))
                {
                    // User doesn't have admin role, redirect to access denied
                    _logger.LogWarning("Non-admin user attempted admin login: {Email}", request.Email);
                    return RedirectToAction("AccessDenied", "Home", new { area = "" });
                }

                // Admin user successfully logged in, redirect to dashboard
                _logger.LogInformation("Admin user successfully logged in: {Email}", request.Email);
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during admin login for email: {Email}", request.Email);
                ModelState.AddModelError("", "خطا در سیستم. لطفاً دوباره تلاش کنید.");
                return View("Index", request);
            }
        }

        /// <summary>
        /// Logout from admin area
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authApiClient.LogoutAsync();
                _logger.LogInformation("Admin user logged out: {UserId}", _currentUser.GetUserId());
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during admin logout");
                return RedirectToAction("Index");
            }
        }
    }

    /// <summary>
    /// Request model for admin login
    /// </summary>
    public class AdminLoginRequest
    {
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل صحیح نیست")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [MinLength(6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}