using Backend.Application.Common.Infrastructure;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Features.Auth.Commands.Login;
using Backend.Application.Features.Auth.Commands.Register;
using Backend.Application.Features.Auth.DTOs;
using Backend.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ICommandDispatcher commandDispatcher,
            IAuthService authService,
            IJwtService jwtService,
            IRefreshTokenService refreshTokenService,
            ILogger<AuthController> logger)
        {
            _commandDispatcher = commandDispatcher;
            _authService = authService;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
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
                var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

                // Store refresh token
                await _refreshTokenService.StoreRefreshTokenAsync(result.UserId!, refreshToken, refreshTokenExpiresAt);

                // Set secure cookie for refresh token
                SetRefreshTokenCookie(refreshToken, refreshTokenExpiresAt);

                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Registration successful",
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
                Message = result.ErrorMessage ?? "Registration failed"
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
                var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(loginDto.RememberMe ? 30 : 7);

                // Store refresh token
                await _refreshTokenService.StoreRefreshTokenAsync(result.UserId!, refreshToken, refreshTokenExpiresAt);

                // Set secure cookie for refresh token
                SetRefreshTokenCookie(refreshToken, refreshTokenExpiresAt);

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

            return BadRequest(new AuthResponseDto
            {
                IsSuccess = false,
                Message = result.ErrorMessage ?? "Invalid username or password"
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    // Revoke all refresh tokens for the user
                    await _refreshTokenService.RevokeAllRefreshTokensAsync(userId);
                }

                // Remove refresh token cookie
                RemoveRefreshTokenCookie();

                return Ok(new { IsSuccess = true, Message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { IsSuccess = false, Message = "Logout failed" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                // Get refresh token from cookie
                var refreshToken = Request.Cookies["refresh_token"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Refresh token not found" });
                }

                // Get user ID from current token (if available) or validate refresh token
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    // Try to get user ID from refresh token (you might need to store this in the token)
                    // For now, we'll require the user to be authenticated
                    return Unauthorized(new { IsSuccess = false, Message = "User not authenticated" });
                }

                // Validate refresh token
                if (!await _refreshTokenService.ValidateRefreshTokenAsync(userId, refreshToken))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Invalid refresh token" });
                }

                // Check for token reuse
                if (await _refreshTokenService.IsRefreshTokenReusedAsync(userId, refreshToken))
                {
                    return BadRequest(new { IsSuccess = false, Message = "Refresh token has been reused" });
                }

                // Get user profile
                var userProfile = await _authService.GetUserProfileAsync(userId);
                if (userProfile == null)
                {
                    return BadRequest(new { IsSuccess = false, Message = "User not found" });
                }

                // Generate new access token
                var roles = await _authService.GetUserRolesAsync(userId);
                var newToken = _jwtService.GenerateToken(userId, userProfile.UserName, userProfile.Email, roles);

                // Generate new refresh token
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

                // Revoke old refresh token
                await _refreshTokenService.RevokeRefreshTokenAsync(userId, refreshToken);

                // Store new refresh token
                await _refreshTokenService.StoreRefreshTokenAsync(userId, newRefreshToken, newRefreshTokenExpiresAt);

                // Set new secure cookie for refresh token
                SetRefreshTokenCookie(newRefreshToken, newRefreshTokenExpiresAt);

                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Token refreshed successfully",
                    UserId = userId,
                    UserName = userProfile.UserName,
                    Email = userProfile.Email,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = _jwtService.GetTokenExpiration(newToken),
                    Roles = roles?.ToList() ?? new List<string>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { IsSuccess = false, Message = "Token refresh failed" });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { IsSuccess = false, Message = "User not authenticated" });
                }

                var profile = await _authService.GetUserProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound(new { IsSuccess = false, Message = "User not found" });
                }

                return Ok(new { IsSuccess = true, Profile = profile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { IsSuccess = false, Message = "Failed to get profile" });
            }
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Only send over HTTPS
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth/refresh-token",
                Expires = expiresAt,
                MaxAge = TimeSpan.FromDays(7)
            };

            Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
        }

        private void RemoveRefreshTokenCookie()
        {
            Response.Cookies.Delete("refresh_token", new CookieOptions
            {
                Path = "/api/auth/refresh-token"
            });
        }
    }
} 