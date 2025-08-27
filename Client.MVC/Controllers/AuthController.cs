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
        [Route("Auth/Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("Auth/Register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto model)
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
        [Route("Auth/Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("Auth/Login")]
        public async Task<IActionResult> Login([FromForm] LoginDto model)
        {
            // Add debug logging
            Logger.LogInformation("Login POST method called");
            Logger.LogInformation("Model: {Model}", model);
            Logger.LogInformation("EmailOrUsername: {Email}", model?.EmailOrUsername);
            Logger.LogInformation("Password: {Password}", model?.Password);
            
            if (!ModelState.IsValid)
            {
                Logger.LogWarning("ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Logger.LogWarning("ModelState Error: {Error}", error.ErrorMessage);
                }
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
    }
} 