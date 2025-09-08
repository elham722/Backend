using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Client.MVC.Services.Abstractions;
using Client.MVC.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers
{
    public class AuthController : SecureController
    {
        private readonly IAuthApiClient _authApiClient;
        private readonly ISessionManager _sessionManager;
        private readonly ILogoutService _logoutService;

        public AuthController(
            IAuthApiClient authApiClient,
            ISessionManager sessionManager,
            ILogoutService logoutService,
            ICurrentUser currentUser,
            IAntiForgeryService antiForgeryService,
            ILogger<AuthController> logger)
            : base(currentUser, antiForgeryService, logger)
        {
            _authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _logoutService = logoutService ?? throw new ArgumentNullException(nameof(logoutService));
        }

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Call AuthApiClient for registration
                var response = await _authApiClient.RegisterAsync(model);

                if (response.IsSuccess)
                {
                    // Set user session using the new session manager
                    _sessionManager.SetUserSession(response);

                    TempData["SuccessMessage"] = "ثبت نام با موفقیت انجام شد!";

                    // Redirect to home with user info
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", response.ErrorMessage ?? "خطا در ثبت نام");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during registration");
                ModelState.AddModelError("", "خطا در ارتباط با سرور");
                return View(model);
            }
        }


        #endregion

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken] // جلوگیری از CSRF
        public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // اطلاعات اضافی امنیتی
                model.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                model.DeviceInfo = Request.Headers["User-Agent"].ToString();

                // Call AuthApiClient for login
                var response = await _authApiClient.LoginAsync(model);

                
                if (response.IsSuccess)
                {
                    // ذخیره سشن و کوکی با session manager جدید
                    _sessionManager.SetUserSession(response);

                    // لاگ امنیتی (اختیاری - برای دیتابیس)
                    Logger.LogInformation("User {User} logged in successfully from IP {IP}",
                        model.EmailOrUsername, model.IpAddress);

                    TempData["SuccessMessage"] = "ورود با موفقیت انجام شد!";

                    // Get user roles after successful login
                    var userRoles = CurrentUser.GetUserRoles();
                    bool isAdmin = userRoles != null && (userRoles.Contains("Admin") || userRoles.Contains("SuperAdmin"));

                    // Check if returnUrl is provided and valid
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        // Check if returnUrl is admin area
                        if (returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                        {
                            if (isAdmin)
                            {
                                // Admin user trying to access admin area - redirect to original admin URL
                                return Redirect(returnUrl);
                            }
                            else
                            {
                                // Regular user trying to access admin area - redirect to access denied
                                return RedirectToAction("AccessDenied", "Home");
                            }
                        }
                        else
                        {
                            // Non-admin returnUrl - redirect to original URL
                            return Redirect(returnUrl);
                        }
                    }
                    
                    // No returnUrl or invalid returnUrl - smart redirection based on user role
                    if (isAdmin)
                    {
                        // Admin user with no specific returnUrl - redirect to admin dashboard
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    else
                    {
                        // Regular user with no specific returnUrl - redirect to home
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                }
                else
                {
                    Logger.LogWarning("Failed login attempt for user {User} from IP {IP}. Error: {Error}",
                        model.EmailOrUsername, model.IpAddress, response.ErrorMessage);

                    ModelState.AddModelError("", response.ErrorMessage ?? "ایمیل یا رمز عبور اشتباه است");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error during login for user {User}", model.EmailOrUsername);
                ModelState.AddModelError("", "خطا در ارتباط با سرور");
                return View(model);
            }
        }

        #endregion

        #region LogOut

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Logout using the dedicated logout service to avoid circular dependencies
                var result = await _logoutService.LogoutAsync();
                
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "خروج با موفقیت انجام شد!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.ErrorMessage ?? "خطا در خروج از سیستم";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during logout");
                TempData["ErrorMessage"] = "خطا در خروج از سیستم";
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }


        #endregion

        #region AccessDenied

        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

    }
} 