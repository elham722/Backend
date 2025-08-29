using Backend.Application.Features.UserManagement.DTOs;
using Microsoft.AspNetCore.Http;

namespace Client.MVC.Services
{
    /// <summary>
    /// Implementation of user session management service
    /// </summary>
    public class UserSessionService : IUserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserSessionService> _logger;

        public UserSessionService(IHttpContextAccessor httpContextAccessor, ILogger<UserSessionService> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Set user session data from authentication result
        /// </summary>
        public void SetUserSession(AuthResultDto result)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("HttpContext is not available");
                    return;
                }

                var session = httpContext.Session;
                var response = httpContext.Response;

                // Store JWT token in both session and HttpOnly cookie
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    // Store in session
                    session.SetString("JWTToken", result.AccessToken);
                    
                    // Store in HttpOnly cookie
                    var jwtCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = _httpContextAccessor.HttpContext?.Request.IsHttps == true, // Only secure in HTTPS
                        SameSite = SameSiteMode.Strict,
                        Expires = result.ExpiresAt ?? DateTime.UtcNow.AddMinutes(60)
                    };
                    response.Cookies.Append("jwt_token", result.AccessToken, jwtCookieOptions);
                    
                    _logger.LogDebug("JWT token stored in session and HttpOnly cookie");
                }

                // Store refresh token in both session and HttpOnly cookie
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    // Store in session
                    session.SetString("RefreshToken", result.RefreshToken);
                    
                    // Store in HttpOnly cookie (longer expiration)
                    var refreshCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = _httpContextAccessor.HttpContext?.Request.IsHttps == true, // Only secure in HTTPS
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(30) // Refresh token lasts longer
                    };
                    response.Cookies.Append("refresh_token", result.RefreshToken, refreshCookieOptions);
                    
                    _logger.LogDebug("Refresh token stored in session and HttpOnly cookie");
                }

                // Store user information in session
                if (result.User != null)
                {
                    session.SetString("UserName", result.User.UserName);
                    session.SetString("UserEmail", result.User.Email);
                    session.SetString("UserId", result.User.Id.ToString());
                    
                    _logger.LogInformation("User session set for user: {UserName} ({Email})", 
                        result.User.UserName, result.User.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user session");
                throw;
            }
        }

        /// <summary>
        /// Clear all user session data and cookies
        /// </summary>
        public void ClearUserSession()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var session = httpContext.Session;
                    var response = httpContext.Response;

                    // Clear session
                    session.Clear();

                    // Clear cookies
                    response.Cookies.Delete("jwt_token");
                    response.Cookies.Delete("refresh_token");

                    _logger.LogInformation("User session and cookies cleared");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user session");
                throw;
            }
        }

        /// <summary>
        /// Get current user ID from session
        /// </summary>
        public string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserId");
        }

        /// <summary>
        /// Get current user name from session
        /// </summary>
        public string? GetUserName()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserName");
        }

        /// <summary>
        /// Get current user email from session
        /// </summary>
        public string? GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("UserEmail");
        }

        /// <summary>
        /// Get JWT token from HttpOnly cookie (fallback to session)
        /// </summary>
        public string? GetJwtToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // Try to get from cookie first (more secure)
            var tokenFromCookie = httpContext.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(tokenFromCookie))
            {
                return tokenFromCookie;
            }

            // Fallback to session
            return httpContext.Session.GetString("JWTToken");
        }

        /// <summary>
        /// Get refresh token from HttpOnly cookie (fallback to session)
        /// </summary>
        public string? GetRefreshToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // Try to get from cookie first (more secure)
            var tokenFromCookie = httpContext.Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(tokenFromCookie))
            {
                return tokenFromCookie;
            }

            // Fallback to session
            return httpContext.Session.GetString("RefreshToken");
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            var jwtToken = GetJwtToken();
            var userId = GetUserId();
            
            // Check if token exists and is not expired
            if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(userId))
            {
                return false;
            }

            // TODO: Add JWT token expiration validation here
            // For now, just check if tokens exist
            
            return true;
        }

        /// <summary>
        /// Get logout data for API call
        /// </summary>
        public LogoutDto GetLogoutDto()
        {
            var refreshToken = GetRefreshToken();
            
            return new LogoutDto
            {
                RefreshToken = refreshToken ?? string.Empty,
                LogoutFromAllDevices = false
            };
        }

        /// <summary>
        /// Refresh JWT token cookie (called after token refresh)
        /// </summary>
        public void RefreshJwtToken(string newToken, DateTime? expiresAt = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                var response = httpContext.Response;
                var session = httpContext.Session;

                // Update session
                session.SetString("JWTToken", newToken);

                // Update cookie
                var jwtCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = expiresAt ?? DateTime.UtcNow.AddMinutes(60)
                };
                response.Cookies.Append("jwt_token", newToken, jwtCookieOptions);

                _logger.LogDebug("JWT token refreshed in session and cookie");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing JWT token");
                throw;
            }
        }
    }
} 