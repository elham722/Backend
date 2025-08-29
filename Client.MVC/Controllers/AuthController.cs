using Backend.Application.Features.UserManagement.DTOs;
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
            ILogger<AuthController> logger)
            : base(userSessionService, logger)
        {
            _authApiClient = authApiClient ?? throw new ArgumentNullException(nameof(authApiClient));
        }

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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Call AuthApiClient for login
                var response = await _authApiClient.LoginAsync(model);

                if (response.IsSuccess)
                {
                    // Set user session using the service
                    UserSessionService.SetUserSession(response);

                    TempData["SuccessMessage"] = "ورود با موفقیت انجام شد!";
                    
                    // Redirect to home with user info
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", response.ErrorMessage ?? "ایمیل یا رمز عبور اشتباه است");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during login");
                ModelState.AddModelError("", "خطا در ارتباط با سرور");
                return View(model);
            }
        }

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

        /// <summary>
        /// Access denied page
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 