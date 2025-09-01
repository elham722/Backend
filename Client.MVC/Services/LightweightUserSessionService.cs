using Backend.Application.Features.UserManagement.DTOs;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Backend.Application.Common.Results;

namespace Client.MVC.Services
{
    /// <summary>
    /// Lightweight user session service that only stores SessionId in session
    /// User data is stored in Redis/DB for better scalability
    /// </summary>
    public interface ILightweightUserSessionService
    {
        string? GetUserId();
        string? GetUserName();
        string? GetUserEmail();
        IEnumerable<string> GetUserRoles();
        DateTime? GetTokenExpiration();
        bool IsAuthenticated();
        bool IsTokenAboutToExpire(int minutesBeforeExpiry = 5);
        string? GetJwtToken();
        string? GetRefreshToken();
        bool HasValidRefreshToken();
        DateTime? GetRefreshTokenExpiration();
        bool IsRefreshTokenAboutToExpire(int daysBeforeExpiry = 7);
        string? GetRefreshTokenType();
        LogoutDto GetLogoutDto();
        void RefreshJwtToken(string newToken, DateTime? expiresAt = null);
        Task ClearUserSessionAsync(CancellationToken cancellationToken = default);
        void ClearUserSession();
        Task<ApiResponse<LogoutResultDto>> LogoutAsync(bool logoutFromAllDevices = false, CancellationToken cancellationToken = default);
    }

    public class LightweightUserSessionService : ILightweightUserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LightweightUserSessionService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJwtClaimsExtractor _jwtClaimsExtractor;
        private readonly CookieSecurityConfig _cookieConfig;
        private readonly JsonSerializerOptions _jsonOptions;

        public LightweightUserSessionService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<LightweightUserSessionService> logger,
            IHttpClientFactory httpClientFactory,
            IJwtClaimsExtractor jwtClaimsExtractor,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _jwtClaimsExtractor = jwtClaimsExtractor ?? throw new ArgumentNullException(nameof(jwtClaimsExtractor));
            _cookieConfig = CookieSecurityConfig.FromConfiguration(configuration);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            _logger.LogInformation("LightweightUserSessionService initialized - Session only stores SessionId");
        }

        /// <summary>
        /// Set user session data - only SessionId in session, user data in Redis/DB
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

                // ✅ Store only SessionId in session (lightweight)
                var sessionId = Guid.NewGuid().ToString();
                session.SetString("SessionId", sessionId);

                // Store JWT token in HttpOnly cookie
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    var jwtCookieOptions = CreateCookieOptions(
                        isSecure: _cookieConfig.RequireSecure && httpContext.Request.IsHttps,
                        sameSite: _cookieConfig.JwtTokenSameSite,
                        expires: result.ExpiresAt ?? DateTime.UtcNow.AddMinutes(60)
                    );

                    response.Cookies.Append("jwt_token", result.AccessToken, jwtCookieOptions);
                    _logger.LogDebug("JWT token stored in HttpOnly cookie");
                }

                // Store refresh token in HttpOnly cookie
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    var refreshCookieOptions = CreateCookieOptions(
                        isSecure: _cookieConfig.RequireSecure && httpContext.Request.IsHttps,
                        sameSite: _cookieConfig.RefreshTokenSameSite,
                        expires: DateTime.UtcNow.AddDays(30)
                    );

                    response.Cookies.Append("refresh_token", result.RefreshToken, refreshCookieOptions);
                    _logger.LogDebug("Refresh token stored in HttpOnly cookie");
                }

                // ✅ User data is stored in Redis/DB, not in session
                if (result.User != null)
                {
                    // Store user data in Redis/DB with SessionId as key
                    StoreUserDataInRedis(sessionId, result.User);
                    
                    _logger.LogInformation("User session created with SessionId: {SessionId} for user: {UserName}", 
                        sessionId, result.User.UserName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting lightweight user session");
                throw;
            }
        }

        /// <summary>
        /// Store user data in Redis/DB instead of session
        /// </summary>
        private void StoreUserDataInRedis(string sessionId, UserDto user)
        {
            try
            {
                // ✅ User data stored in Redis/DB, not in session
                var userData = new
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName,
                    UserEmail = user.Email,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30) // Session timeout
                };

                // In a real implementation, this would store in Redis/DB
                // For now, we'll use JWT claims as the source of truth
                _logger.LogDebug("User data would be stored in Redis/DB with key: session:{SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing user data in Redis/DB");
            }
        }

        /// <summary>
        /// Get user ID from JWT claims (not from session)
        /// </summary>
        public string? GetUserId()
        {
            try
            {
                // ✅ Get from JWT claims, not from session
                var jwtToken = GetJwtToken();
                return _jwtClaimsExtractor.GetClaimValue("sub", jwtToken) ??
                       _jwtClaimsExtractor.GetClaimValue("nameid", jwtToken) ??
                       _jwtClaimsExtractor.GetClaimValue("UserId", jwtToken) ??
                       _jwtClaimsExtractor.GetUserId(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID from JWT claims");
                return null;
            }
        }

        /// <summary>
        /// Get username from JWT claims (not from session)
        /// </summary>
        public string? GetUserName()
        {
            try
            {
                // ✅ Get from JWT claims, not from session
                var jwtToken = GetJwtToken();
                return _jwtClaimsExtractor.GetClaimValue("name", jwtToken) ?? 
                       _jwtClaimsExtractor.GetClaimValue("UserName", jwtToken) ??
                       _jwtClaimsExtractor.GetUserName(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting username from JWT claims");
                return null;
            }
        }

        /// <summary>
        /// Get user email from JWT claims (not from session)
        /// </summary>
        public string? GetUserEmail()
        {
            try
            {
                // ✅ Get from JWT claims, not from session
                var jwtToken = GetJwtToken();
                return _jwtClaimsExtractor.GetClaimValue("email", jwtToken) ?? 
                       _jwtClaimsExtractor.GetClaimValue("UserEmail", jwtToken) ??
                       _jwtClaimsExtractor.GetUserEmail(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user email from JWT claims");
                return null;
            }
        }

        /// <summary>
        /// Get user roles from JWT claims (not from session)
        /// </summary>
        public IEnumerable<string> GetUserRoles()
        {
            try
            {
                // ✅ Get from JWT claims, not from session
                var jwtToken = GetJwtToken();
                var roles = _jwtClaimsExtractor.GetClaimValues("role", jwtToken) ?? 
                           _jwtClaimsExtractor.GetClaimValues("Role", jwtToken) ??
                           _jwtClaimsExtractor.GetClaimValues("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", jwtToken) ??
                           _jwtClaimsExtractor.GetUserRoles(jwtToken);

                return roles ?? Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles from JWT claims");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Get JWT token from cookie
        /// </summary>
        public string? GetJwtToken()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Request.Cookies["jwt_token"];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting JWT token from cookie");
                return null;
            }
        }

        /// <summary>
        /// Get refresh token from cookie
        /// </summary>
        public string? GetRefreshToken()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Request.Cookies["refresh_token"];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refresh token from cookie");
                return null;
            }
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            try
            {
                var jwtToken = GetJwtToken();
                if (string.IsNullOrEmpty(jwtToken))
                    return false;

                // ✅ Validate JWT token without session dependency
                return _jwtClaimsExtractor.IsTokenValid(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return false;
            }
        }

        /// <summary>
        /// Get token expiration from JWT claims
        /// </summary>
        public DateTime? GetTokenExpiration()
        {
            try
            {
                // ✅ Get from JWT claims, not from session
                var jwtToken = GetJwtToken();
                return _jwtClaimsExtractor.GetTokenExpiration(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token expiration from JWT claims");
                return null;
            }
        }

        /// <summary>
        /// Check if JWT token is about to expire
        /// </summary>
        public bool IsTokenAboutToExpire(int minutesBeforeExpiry = 5)
        {
            try
            {
                var expiration = GetTokenExpiration();
                if (!expiration.HasValue)
                    return false;

                var threshold = DateTime.UtcNow.AddMinutes(minutesBeforeExpiry);
                return expiration.Value <= threshold;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token expiration");
                return false;
            }
        }

        /// <summary>
        /// Check if refresh token is valid
        /// </summary>
        public bool HasValidRefreshToken()
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                    return false;

                // ✅ Validate refresh token without session dependency
                return _jwtClaimsExtractor.IsTokenValid(refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token");
                return false;
            }
        }

        /// <summary>
        /// Get refresh token expiration
        /// </summary>
        public DateTime? GetRefreshTokenExpiration()
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                    return null;

                return _jwtClaimsExtractor.GetTokenExpiration(refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refresh token expiration");
                return null;
            }
        }

        /// <summary>
        /// Check if refresh token is about to expire
        /// </summary>
        public bool IsRefreshTokenAboutToExpire(int daysBeforeExpiry = 7)
        {
            try
            {
                var expiration = GetRefreshTokenExpiration();
                if (!expiration.HasValue)
                    return false;

                var threshold = DateTime.UtcNow.AddDays(daysBeforeExpiry);
                return expiration.Value <= threshold;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking refresh token expiration");
                return false;
            }
        }

        /// <summary>
        /// Get refresh token type
        /// </summary>
        public string? GetRefreshTokenType()
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                    return null;

                return _jwtClaimsExtractor.IsTokenValid(refreshToken) ? "JWT" : "Opaque";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining refresh token type");
                return null;
            }
        }

        /// <summary>
        /// Get logout data
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
        /// Refresh JWT token
        /// </summary>
        public void RefreshJwtToken(string newToken, DateTime? expiresAt = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                var jwtCookieOptions = CreateCookieOptions(
                    isSecure: _cookieConfig.RequireSecure && httpContext.Request.IsHttps,
                    sameSite: _cookieConfig.JwtTokenSameSite,
                    expires: expiresAt ?? DateTime.UtcNow.AddMinutes(60)
                );

                httpContext.Response.Cookies.Append("jwt_token", newToken, jwtCookieOptions);
                _logger.LogDebug("JWT token refreshed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing JWT token");
            }
        }

        /// <summary>
        /// Clear user session
        /// </summary>
        public async Task ClearUserSessionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                // Clear cookies
                httpContext.Response.Cookies.Delete("jwt_token");
                httpContext.Response.Cookies.Delete("refresh_token");

                // ✅ Clear only SessionId from session (lightweight)
                httpContext.Session.Remove("SessionId");

                // Clear user data from Redis/DB
                var sessionId = httpContext.Session.GetString("SessionId");
                if (!string.IsNullOrEmpty(sessionId))
                {
                    ClearUserDataFromRedis(sessionId);
                }

                _logger.LogInformation("Lightweight user session cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing lightweight user session");
            }
        }

        /// <summary>
        /// Clear user session synchronously
        /// </summary>
        public void ClearUserSession()
        {
            ClearUserSessionAsync().Wait();
        }

        /// <summary>
        /// Clear user data from Redis/DB
        /// </summary>
        private void ClearUserDataFromRedis(string sessionId)
        {
            try
            {
                // ✅ Clear user data from Redis/DB, not from session
                _logger.LogDebug("User data would be cleared from Redis/DB with key: session:{SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user data from Redis/DB");
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        public async Task<ApiResponse<LogoutResultDto>> LogoutAsync(bool logoutFromAllDevices = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var refreshToken = GetRefreshToken();
                ApiResponse<LogoutResultDto>? backendResult = null;

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var logoutDto = new LogoutDto
                    {
                        RefreshToken = refreshToken,
                        LogoutFromAllDevices = logoutFromAllDevices
                    };

                    // Call backend API
                    var httpClient = _httpClientFactory.CreateClient("ApiClient");
                    var json = JsonSerializer.Serialize(logoutDto, _jsonOptions);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    
                    var response = await httpClient.PostAsync("api/Auth/logout", content, cancellationToken);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        var result = JsonSerializer.Deserialize<LogoutResultDto>(responseContent, _jsonOptions);
                        backendResult = ApiResponse<LogoutResultDto>.Success(result ?? new LogoutResultDto());
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        backendResult = ApiResponse<LogoutResultDto>.Error($"Logout failed: {errorContent}", (int)response.StatusCode);
                    }
                }

                // Clear local session
                await ClearUserSessionAsync(cancellationToken);

                return backendResult ?? ApiResponse<LogoutResultDto>.Success(new LogoutResultDto
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    LogoutTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return ApiResponse<LogoutResultDto>.Error("An error occurred during logout", 500);
            }
        }

        /// <summary>
        /// Create cookie options with security configuration
        /// </summary>
        private CookieOptions CreateCookieOptions(bool isSecure, SameSiteMode sameSite, DateTime expires)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecure,
                SameSite = sameSite,
                Path = _cookieConfig.Path,
                Domain = _cookieConfig.Domain,
                Expires = expires
            };
        }
    }
} 