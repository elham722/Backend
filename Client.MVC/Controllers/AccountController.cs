using Microsoft.AspNetCore.Mvc;
using Backend.Infrastructure.ExternalServices;
using Backend.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Http;

namespace Client.MVC.Controllers
{
    public class AccountController(IExternalService externalService) : Controller
    {
        private readonly IExternalService _externalService = externalService;

        #region Login

        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            try
            {
                var response = await _externalService.PostAsync<LoginDto, dynamic>("api/auth/login", loginDto);

                // Store token in session (since we can't use localStorage in server-side)
                var token = response.GetProperty("token").GetString();
                var userName = response.GetProperty("userName").GetString();

                // Use byte array conversion for session storage
                var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token ?? string.Empty);
                var userNameBytes = System.Text.Encoding.UTF8.GetBytes(userName ?? string.Empty);

                HttpContext.Session.Set("JWTToken", tokenBytes);
                HttpContext.Session.Set("UserName", userNameBytes);

                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(loginDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(loginDto);
            }
        }



        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            try
            {
                var response = await _externalService.PostAsync<RegisterDto, dynamic>("api/auth/register", registerDto);

                // Store token in session
                var token = response.GetProperty("token").GetString();
                var userName = response.GetProperty("userName").GetString();

                // Use byte array conversion for session storage
                var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token ?? string.Empty);
                var userNameBytes = System.Text.Encoding.UTF8.GetBytes(userName ?? string.Empty);

                HttpContext.Session.Set("JWTToken", tokenBytes);
                HttpContext.Session.Set("UserName", userNameBytes);

                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(registerDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(registerDto);
            }
        }


        #endregion

        #region Logout

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _externalService.PostAsync<object>("api/auth/logout");
            }
            catch
            {
                // Ignore errors during logout
            }

            // Clear session
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logged out successfully!";
            return RedirectToAction("Index", "Home");
        }

        #endregion


    }
} 