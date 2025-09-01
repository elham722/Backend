using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Client.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace Client.MVC.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthApiClient _authApiClient;

        public AuthController(
            IAuthApiClient authApiClient, 
            IUserSessionService userSessionService,
            ILogger<AuthController> logger,
            IErrorHandlingService errorHandlingService,
            ICacheService cacheService,
            IAntiForgeryService antiForgeryService)
            : base(userSessionService, logger, errorHandlingService, cacheService, antiForgeryService)
        {
            _authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
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
                    // Set user session using the service
                    UserSessionService.SetUserSession(response);

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
                    // ذخیره سشن و کوکی
                    UserSessionService.SetUserSession(response);

                    // لاگ امنیتی (اختیاری - برای دیتابیس)
                    Logger.LogInformation("User {User} logged in successfully from IP {IP}",
                        model.EmailOrUsername, model.IpAddress);

                    TempData["SuccessMessage"] = "ورود با موفقیت انجام شد!";

                    // اگر returnUrl معتبر باشه برگرد همونجا، در غیر اینصورت برو Home
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
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
                // Get logout DTO with current refresh token
                var logoutDto = UserSessionService.GetLogoutDto();

                // Call AuthApiClient for logout
                await _authApiClient.LogoutAsync(logoutDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during logout");
                // Continue with local logout even if API call fails
            }

            // Clear session using the service
            UserSessionService.ClearUserSession();
            TempData["SuccessMessage"] = "خروج با موفقیت انجام شد!";
            return RedirectToAction("Index", "Home");
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