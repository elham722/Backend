using Microsoft.AspNetCore.Mvc;
using Backend.Infrastructure.ExternalServices;
using Backend.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Client.MVC.Services;

namespace Client.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IExternalService _externalService;
        private readonly ITokenService _tokenService;

        public AccountController(IExternalService externalService, ITokenService tokenService)
        {
            _externalService = externalService;
            _tokenService = tokenService;
        }

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
                var response = await _externalService.PostAsync<LoginDto, AuthResponseDto>("api/auth/login", loginDto);

                if (response.IsSuccess && !string.IsNullOrEmpty(response.Token))
                {
                    // Store tokens using token service
                    _tokenService.StoreTokens(
                        response.Token,
                        response.RefreshToken ?? string.Empty,
                        response.UserName ?? string.Empty,
                        response.UserId ?? string.Empty,
                        response.ExpiresAt ?? DateTime.UtcNow.AddHours(1)
                    );

                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", response.Message ?? "Login failed");
                    return View(loginDto);
                }
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
                var response = await _externalService.PostAsync<RegisterDto, AuthResponseDto>("api/auth/register", registerDto);

                if (response.IsSuccess && !string.IsNullOrEmpty(response.Token))
                {
                    // Store tokens using token service
                    _tokenService.StoreTokens(
                        response.Token,
                        response.RefreshToken ?? string.Empty,
                        response.UserName ?? string.Empty,
                        response.UserId ?? string.Empty,
                        response.ExpiresAt ?? DateTime.UtcNow.AddHours(1)
                    );

                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (response.Errors?.Any() == true)
                    {
                        foreach (var error in response.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", response.Message ?? "Registration failed");
                    }
                    return View(registerDto);
                }
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

            // Clear tokens using token service
            _tokenService.ClearTokens();
            TempData["SuccessMessage"] = "Logged out successfully!";
            return RedirectToAction("Index", "Home");
        }

        #endregion


    }
} 