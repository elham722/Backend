using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Backend.Identity.Models;
using Backend.Application.Features.Auth.DTOs;
using Backend.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

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
                return BadRequest(new AuthResponseDto 
                { 
                    IsSuccess = false,
                    Message = "Passwords do not match",
                    Errors = new List<string> { "Password confirmation does not match" }
                });
            }

            var user = ApplicationUser.Create(registerDto.Email, registerDto.UserName);
            user.PhoneNumber = registerDto.PhoneNumber;

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            
            if (result.Succeeded)
            {
                // Get user roles (default to empty list for new users)
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();
                
                return Ok(new AuthResponseDto
                { 
                    IsSuccess = true,
                    Message = "User registered successfully",
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = _jwtService.GetTokenExpiration(token),
                    Roles = roles.ToList()
                });
            }
            
            return BadRequest(new AuthResponseDto 
            { 
                IsSuccess = false,
                Message = "Registration failed",
                Errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
            {
                return BadRequest(new AuthResponseDto 
                { 
                    IsSuccess = false,
                    Message = "Invalid username or password"
                });
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                // Update last login
                user.UpdateLastLogin();
                await _userManager.UpdateAsync(user);

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();
                
                return Ok(new AuthResponseDto
                { 
                    IsSuccess = true,
                    Message = "Login successful",
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = _jwtService.GetTokenExpiration(token),
                    Roles = roles.ToList()
                });
            }
            
            if (result.IsLockedOut)
            {
                return BadRequest(new AuthResponseDto 
                { 
                    IsSuccess = false,
                    Message = "Account is locked out"
                });
            }
            
            if (result.RequiresTwoFactor)
            {
                return BadRequest(new AuthResponseDto 
                { 
                    IsSuccess = false,
                    Message = "Two-factor authentication required"
                });
            }
            
            return BadRequest(new AuthResponseDto 
            { 
                IsSuccess = false,
                Message = "Invalid username or password"
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new AuthResponseDto 
            { 
                IsSuccess = true,
                Message = "Logged out successfully"
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new RefreshTokenResponseDto 
                { 
                    IsSuccess = false,
                    Message = "Refresh token is required"
                });
            }

            // Validate refresh token
            if (!_jwtService.ValidateRefreshToken(request.RefreshToken))
            {
                return BadRequest(new RefreshTokenResponseDto 
                { 
                    IsSuccess = false,
                    Message = "Invalid refresh token"
                });
            }

            // Get current user from token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new RefreshTokenResponseDto 
                { 
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new RefreshTokenResponseDto 
                { 
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            // Generate new tokens
            var roles = await _userManager.GetRolesAsync(user);
            var newToken = _jwtService.GenerateToken(user.Id, user.UserName, user.Email, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            return Ok(new RefreshTokenResponseDto
            {
                IsSuccess = true,
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = _jwtService.GetTokenExpiration(newToken),
                Message = "Token refreshed successfully"
            });
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