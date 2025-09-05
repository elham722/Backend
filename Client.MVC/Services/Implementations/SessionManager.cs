using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Client.MVC.Services.Abstractions;
using System.Text.Json;

namespace Client.MVC.Services.Implementations
{
    /// <summary>
    /// Implementation of ISessionManager that handles user session lifecycle
    /// Uses secure cookies for token storage - stateless approach
    /// No API dependencies to avoid circular dependency issues
    /// </summary>
    public class SessionManager : ISessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<SessionManager> _logger;
        private readonly CookieSecurityConfig _cookieConfig;

        public SessionManager(
            IHttpContextAccessor httpContextAccessor,
            ITokenProvider tokenProvider,
            ILogger<SessionManager> logger,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cookieConfig = CookieSecurityConfig.FromConfiguration(configuration);
        }

        /// <summary>
        /// Set user session data from authentication result
        /// </summary>
        public void SetUserSession(LoginResponse result)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogError("HttpContext is null, cannot set user session");
                    return;
                }

                // Set JWT token in HTTP-only cookie
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    SetSecureCookie(httpContext, "JwtToken", result.AccessToken, result.ExpiresAt);
                }

                // Set refresh token in HTTP-only cookie
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    SetSecureCookie(httpContext, "RefreshToken", result.RefreshToken, 
                        DateTime.UtcNow.AddDays(30)); // Refresh token typically lasts longer
                }

                _logger.LogInformation("User session set successfully for user: {UserId}", result.User?.Id ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set user session");
                throw;
            }
        }

        /// <summary>
        /// Set user session data from API response
        /// </summary>
        public void SetUserSession(ApiResponse<LoginResponse> response)
        {
            if (response.IsSuccess && response.Data != null)
            {
                SetUserSession(response.Data);
            }
            else
            {
                _logger.LogWarning("Cannot set user session from failed API response: {Error}", response.ErrorMessage);
            }
        }

        /// <summary>
        /// Clear all user session data
        /// </summary>
        public void ClearUserSession()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogError("HttpContext is null, cannot clear user session");
                    return;
                }

                // Clear JWT token cookie
                httpContext.Response.Cookies.Delete("JwtToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = _cookieConfig.RequireSecure,
                    SameSite = _cookieConfig.JwtTokenSameSite,
                    Path = _cookieConfig.Path
                });

                // Clear refresh token cookie
                httpContext.Response.Cookies.Delete("RefreshToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = _cookieConfig.RequireSecure,
                    SameSite = _cookieConfig.RefreshTokenSameSite,
                    Path = _cookieConfig.Path
                });

                _logger.LogInformation("User session cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear user session");
                throw;
            }
        }

        /// <summary>
        /// Clear all user session data asynchronously
        /// </summary>
        public Task ClearUserSessionAsync(CancellationToken cancellationToken = default)
        {
            ClearUserSession();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Note: LogoutAsync method moved to LogoutService to avoid circular dependencies
        /// SessionManager now only handles local session management
        /// </summary>

        /// <summary>
        /// Get logout DTO with current refresh token
        /// </summary>
        public LogoutDto GetLogoutDto()
        {
            var refreshToken = _tokenProvider.GetRefreshToken();
            return new LogoutDto
            {
                RefreshToken = refreshToken ?? string.Empty,
                LogoutFromAllDevices = false
            };
        }

        /// <summary>
        /// Set secure cookie with proper security settings
        /// </summary>
        private void SetSecureCookie(HttpContext httpContext, string name, string value, DateTime? expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = _cookieConfig.RequireSecure,
                SameSite = name == "JwtToken" ? _cookieConfig.JwtTokenSameSite : _cookieConfig.RefreshTokenSameSite,
                Path = _cookieConfig.Path,
                Expires = expiresAt
            };

            if (!string.IsNullOrEmpty(_cookieConfig.Domain))
            {
                cookieOptions.Domain = _cookieConfig.Domain;
            }

            httpContext.Response.Cookies.Append(name, value, cookieOptions);
        }
    }
}