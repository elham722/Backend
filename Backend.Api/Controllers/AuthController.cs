using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Backend.Identity.Models;
using Backend.Application.Features.Auth.DTOs;
using Backend.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            var user = ApplicationUser.Create(registerDto.Email, registerDto.UserName);
            user.PhoneNumber = registerDto.PhoneNumber;

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            
            if (result.Succeeded)
            {
                // Get user roles (default to empty list for new users)
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);
                
                return Ok(new 
                { 
                    message = "User registered successfully",
                    userId = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    token = token
                });
            }
            
            return BadRequest(new { message = "Registration failed", errors = result.Errors });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid username or password" });
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);
                
                return Ok(new 
                { 
                    message = "Login successful",
                    userId = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    token = token
                });
            }
            
            if (result.IsLockedOut)
            {
                return BadRequest(new { message = "Account is locked out" });
            }
            
            if (result.RequiresTwoFactor)
            {
                return BadRequest(new { message = "Two-factor authentication required" });
            }
            
            return BadRequest(new { message = "Invalid username or password" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return Unauthorized();
            }

            var profile = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.Account.CreatedAt
            };

            return Ok(profile);
        }
    }
} 