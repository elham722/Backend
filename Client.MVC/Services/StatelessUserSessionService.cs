using Backend.Application.Features.UserManagement.DTOs;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Client.MVC.Services
{
    /// <summary>
    /// Stateless user session service that uses only JWT claims and cookies
    /// No session storage - completely stateless
    /// </summary>
    public interface IStatelessUserSessionService
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

    public class StatelessUserSessionService : IStatelessUserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<StatelessUserSessionService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJwtClaimsExtractor _jwtClaimsExtractor;
        private readonly CookieSecurityConfig _cookieConfig;
        private readonly JsonSerializerOptions _jsonOptions;

        public StatelessUserSessionService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<StatelessUserSessionService> logger,
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
            
            _logger.LogInformation("StatelessUserSessionService initialized - No session storage used");
        }

        /// <summary>
        /// Get current user ID from JWT claims
        /// </summary>
        public string? GetUserId()
        {
            var jwtToken = GetJwtToken();
            return _jwtClaimsExtractor.GetUserId(jwtToken);
        }

        /// <summary>
        /// Get current user name from JWT claims
        /// </summary>
        public string? GetUserName()
        {
            var jwtToken = GetJwtToken();
            return _jwtClaimsExtractor.GetUserName(jwtToken);
        }

        /// <summary>
        /// Get current user email from JWT claims
        /// </summary>
        public string? GetUserEmail()
        {
            var jwtToken = GetJwtToken();
            return _jwtClaimsExtractor.GetUserEmail(jwtToken);
        }

        /// <summary>
        /// Get user roles from JWT claims
        /// </summary>
        public IEnumerable<string> GetUserRoles()
        {
            var jwtToken = GetJwtToken();
            return _jwtClaimsExtractor.GetUserRoles(jwtToken);
        }

        /// <summary>
        /// Get JWT token expiration from claims
        /// </summary>
        public DateTime? GetTokenExpiration()
        {
            var jwtToken = GetJwtToken();
            return _jwtClaimsExtractor.GetTokenExpiration(jwtToken);
        }

        /// <summary>
        /// Check if user is authenticated using JWT claims
        /// </summary>
        public bool IsAuthenticated()
        {
            var jwtToken = GetJwtToken();
            return _jwtClaimsExtractor.IsTokenValid(jwtToken);
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
        /// Get JWT token from HttpOnly cookie
        /// </summary>
        public string? GetJwtToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            return httpContext.Request.Cookies["jwt_token"];
        }

        /// <summary>
        /// Get refresh token from HttpOnly cookie
        /// </summary>
        public string? GetRefreshToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            return httpContext.Request.Cookies["refresh_token"];
        }

        /// <summary>
        /// Check if refresh token exists and is valid
        /// </summary>
        public bool HasValidRefreshToken()
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                    return false;

                // Basic validation: check if token has reasonable length
                if (refreshToken.Length < 32)
                {
                    _logger.LogWarning("Refresh token seems too short, might be invalid");
                    return false;
                }

                // Try to validate as JWT token if possible
                var expiration = GetRefreshTokenExpiration();
                if (expiration.HasValue)
                {
                    // It's a JWT token, check expiration
                    if (expiration.Value <= DateTime.UtcNow)
                    {
                        _logger.LogDebug("Refresh token (JWT) has expired at {ExpirationTime}", expiration.Value);
                        return false;
                    }
                    
                    _logger.LogDebug("Refresh token (JWT) validation passed. Expires at {ExpirationTime}", expiration.Value);
                }
                else
                {
                    // It's an opaque token, just check length
                    _logger.LogDebug("Refresh token (opaque) validation passed");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token");
                return false;
            }
        }

        /// <summary>
        /// Get refresh token expiration time (if it's a JWT token)
        /// </summary>
        public DateTime? GetRefreshTokenExpiration()
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                    return null;

                // Try to parse as JWT token first
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                if (handler.CanReadToken(refreshToken))
                {
                    var token = handler.ReadJwtToken(refreshToken);
                    return token.ValidTo;
                }

                // If not a JWT, return null (refresh tokens might be opaque tokens)
                return null;
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
        /// Get refresh token type (JWT or Opaque)
        /// </summary>
        public string? GetRefreshTokenType()
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                    return null;

                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                return handler.CanReadToken(refreshToken) ? "JWT" : "Opaque";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining refresh token type");
                return null;
            }
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

                // Update cookie only
                var jwtCookieOptions = CreateCookieOptions(
                    isSecure: _cookieConfig.RequireSecure && httpContext.Request.IsHttps,
                    sameSite: _cookieConfig.JwtTokenSameSite,
                    expires: expiresAt ?? DateTime.UtcNow.AddMinutes(60)
                );
                response.Cookies.Append("jwt_token", newToken, jwtCookieOptions);

                _logger.LogDebug("JWT token refreshed in cookie with SameSite: {SameSite}", 
                    _cookieConfig.JwtTokenSameSite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing JWT token");
                throw;
            }
        }

        /// <summary>
        /// Clear all user session data and cookies (stateless - only cookies)
        /// </summary>
        public async Task ClearUserSessionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                // First, try to invalidate refresh token on backend
                await InvalidateRefreshTokenOnBackendAsync(cancellationToken);

                // Then clear cookies only (no session to clear)
                var response = httpContext.Response;

                // Clear cookies
                response.Cookies.Delete("jwt_token");
                response.Cookies.Delete("refresh_token");

                _logger.LogInformation("User session cleared successfully (stateless)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user session");
                throw;
            }
        }

        /// <summary>
        /// Clear user session data (synchronous version)
        /// </summary>
        public void ClearUserSession()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                var response = httpContext.Response;

                // Clear cookies only (no session to clear)
                response.Cookies.Delete("jwt_token");
                response.Cookies.Delete("refresh_token");

                _logger.LogInformation("User session cleared successfully (stateless)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user session");
                throw;
            }
        }

        /// <summary>
        /// Logout user with options
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

                    // Use HttpClientFactory to make direct API call
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
                    
                    _logger.LogInformation("User logout from backend completed. Success: {Success}, LogoutFromAllDevices: {LogoutFromAllDevices}", 
                        backendResult.IsSuccess, logoutFromAllDevices);
                }

                // Always clear local cookies
                await ClearUserSessionAsync(cancellationToken);

                // Return backend result if available, otherwise create success result
                if (backendResult != null)
                {
                    return backendResult;
                }

                var logoutResult = new LogoutResultDto
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    LogoutTime = DateTime.UtcNow
                };
                return ApiResponse<LogoutResultDto>.Success(logoutResult);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("User logout was cancelled");
                // Still clear local data even if cancelled
                await ClearUserSessionAsync(cancellationToken);
                
                return ApiResponse<LogoutResultDto>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                // Still clear local data even if backend call fails
                await ClearUserSessionAsync(cancellationToken);
                
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

        /// <summary>
        /// Invalidate refresh token on backend
        /// </summary>
        private async Task InvalidateRefreshTokenOnBackendAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogDebug("No refresh token to invalidate");
                    return;
                }

                var logoutDto = GetLogoutDto();
                
                // Use HttpClientFactory to make direct API call
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var json = JsonSerializer.Serialize(logoutDto, _jsonOptions);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync("api/Auth/logout", content, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Refresh token invalidated on backend");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Failed to invalidate refresh token on backend. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Refresh token invalidation was cancelled");
                // Don't throw - cancellation is expected
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate refresh token on backend. Token may still be valid.");
                // Don't throw - this is not critical for local logout
            }
        }
    }
} 