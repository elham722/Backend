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
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null)
                {
                    _logger.LogWarning("Session is not available");
                    return;
                }

                // Store JWT token
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    session.SetString("JWTToken", result.AccessToken);
                    _logger.LogDebug("JWT token stored in session");
                }

                // Store refresh token
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    session.SetString("RefreshToken", result.RefreshToken);
                    _logger.LogDebug("Refresh token stored in session");
                }

                // Store user information
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
        /// Clear all user session data
        /// </summary>
        public void ClearUserSession()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session != null)
                {
                    session.Clear();
                    _logger.LogInformation("User session cleared");
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
        /// Get JWT token from session
        /// </summary>
        public string? GetJwtToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");
        }

        /// <summary>
        /// Get refresh token from session
        /// </summary>
        public string? GetRefreshToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("RefreshToken");
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            var jwtToken = GetJwtToken();
            var userId = GetUserId();
            
            return !string.IsNullOrEmpty(jwtToken) && !string.IsNullOrEmpty(userId);
        }

        /// <summary>
        /// Get logout DTO with current refresh token
        /// </summary>
        public LogoutDto GetLogoutDto()
        {
            var refreshToken = GetRefreshToken();
            
            return new LogoutDto
            {
                RefreshToken = refreshToken,
                LogoutFromAllDevices = false
            };
        }
    }
} 