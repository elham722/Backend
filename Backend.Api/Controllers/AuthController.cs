using Microsoft.AspNetCore.Mvc;
using Backend.Application.Features.Auth.DTOs;
using Backend.Application.Features.Auth.Commands.Login;
using Backend.Application.Features.Auth.Commands.Register;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Identity.Services;
using System.Security.Claims;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(
            ICommandDispatcher commandDispatcher,
            IAuthService authService,
            IJwtService jwtService)
        {
            _commandDispatcher = commandDispatcher;
            _authService = authService;
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

            var command = new RegisterCommand
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Password = registerDto.Password,
                PhoneNumber = registerDto.PhoneNumber
            };

            var result = await _commandDispatcher.DispatchAsync(command);
            
            if (result.IsSuccess)
            {
                var refreshToken = _jwtService.GenerateRefreshToken();
                
                return Ok(new AuthResponseDto
                { 
                    IsSuccess = true,
                    Message = "User registered successfully",
                    UserId = result.UserId,
                    UserName = result.UserName,
                    Email = result.Email,
                    Token = result.Token,
                    RefreshToken = refreshToken,
                    ExpiresAt = result.ExpiresAt,
                    Roles = result.Roles ?? new List<string>()
                });
            }
            
            return BadRequest(new AuthResponseDto 
            { 
                IsSuccess = false,
                Message = result.ErrorMessage ?? "Registration failed",
                Errors = new List<string> { result.ErrorMessage ?? "Registration failed" }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var command = new LoginCommand
            {
                UserName = loginDto.UserName,
                Password = loginDto.Password,
                RememberMe = loginDto.RememberMe
            };

            var result = await _commandDispatcher.DispatchAsync(command);
            
            if (result.IsSuccess)
            {
                var refreshToken = _jwtService.GenerateRefreshToken();
                
                return Ok(new AuthResponseDto
                { 
                    IsSuccess = true,
                    Message = "Login successful",
                    UserId = result.UserId,
                    UserName = result.UserName,
                    Email = result.Email,
                    Token = result.Token,
                    RefreshToken = refreshToken,
                    ExpiresAt = result.ExpiresAt,
                    Roles = result.Roles ?? new List<string>()
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
                Message = result.ErrorMessage ?? "Invalid username or password"
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogoutAsync();
            return Ok(new AuthResponseDto 
            { 
                IsSuccess = result,
                Message = result ? "Logged out successfully" : "Logout failed"
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

            var result = await _authService.RefreshTokenAsync(request.RefreshToken, userId);

            if (result.IsSuccess)
            {
                return Ok(new RefreshTokenResponseDto
                {
                    IsSuccess = true,
                    Token = result.Token,
                    RefreshToken = result.RefreshToken,
                    ExpiresAt = result.ExpiresAt,
                    Message = "Token refreshed successfully"
                });
            }

            return BadRequest(new RefreshTokenResponseDto 
            { 
                IsSuccess = false,
                Message = result.Message ?? "Invalid refresh token"
            });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var profile = await _authService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                return Unauthorized();
            }

            return Ok(profile);
        }
    }
} 