using Backend.Application.Features.UserManagement.DTOs;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Backend.Application.Common.Results;

namespace Client.MVC.Services
{
    /// <summary>
    /// Configuration for cookie security settings
    /// </summary>
    public class CookieSecurityConfig
    {
        public SameSiteMode JwtTokenSameSite { get; set; } = SameSiteMode.Strict;
        public SameSiteMode RefreshTokenSameSite { get; set; } = SameSiteMode.Lax;
        public bool RequireSecure { get; set; } = true;
        public string? Domain { get; set; }
        public string Path { get; set; } = "/";

        /// <summary>
        /// Create configuration from IConfiguration
        /// </summary>
        public static CookieSecurityConfig FromConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetSection("CookieSecurity");
            
            return new CookieSecurityConfig
            {
                JwtTokenSameSite = ParseSameSite(section["JwtTokenSameSite"], SameSiteMode.Strict),
                RefreshTokenSameSite = ParseSameSite(section["RefreshTokenSameSite"], SameSiteMode.Lax),
                RequireSecure = section.GetValue<bool>("RequireSecure", true),
                Domain = section["Domain"],
                Path = section["Path"] ?? "/"
            };
        }

        private static SameSiteMode ParseSameSite(string? value, SameSiteMode defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value.ToLower() switch
            {
                "strict" => SameSiteMode.Strict,
                "lax" => SameSiteMode.Lax,
                "none" => SameSiteMode.None,
                _ => defaultValue
            };
        }
    }

    /// <summary>
    /// Implementation of user session management service
    /// </summary>
    public class UserSessionService : IUserSessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserSessionService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CookieSecurityConfig _cookieConfig;
        private readonly JsonSerializerOptions _jsonOptions;
        
        // Token expiration cache for optimization
        private DateTimeOffset? _cachedTokenExpiration;
        private string? _cachedTokenHash;

        public UserSessionService(
            IHttpContextAccessor httpContextAccessor, 
            ILogger<UserSessionService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cookieConfig = CookieSecurityConfig.FromConfiguration(configuration);
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            _logger.LogInformation("UserSessionService initialized with JWT SameSite: {JwtSameSite}, Refresh SameSite: {RefreshSameSite}", 
                _cookieConfig.JwtTokenSameSite, _cookieConfig.RefreshTokenSameSite);
        }

        /// <summary>
        /// Set user session data from authentication result
        /// </summary>
        public void SetUserSession(AuthResultDto result)
        {
            SetUserSessionInternal(result);
        }

        /// <summary>
        /// Set user session data from API response
        /// </summary>
        public void SetUserSession(ApiResponse<AuthResultDto> response)
        {
            if (response?.IsSuccess == true && response.Data != null)
            {
                SetUserSessionInternal(response.Data);
            }
            else
            {
                _logger.LogWarning("Cannot set user session from failed API response");
            }
        }

        /// <summary>
        /// Internal method to set user session data
        /// </summary>
        private void SetUserSessionInternal(AuthResultDto result)
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

                // Store JWT token in HttpOnly cookie only
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    var jwtCookieOptions = CreateCookieOptions(
                        isSecure: _cookieConfig.RequireSecure && httpContext.Request.IsHttps,
                        sameSite: _cookieConfig.JwtTokenSameSite,
                        expires: result.ExpiresAt ?? DateTime.UtcNow.AddMinutes(60)
                    );
                    
                    response.Cookies.Append("jwt_token", result.AccessToken, jwtCookieOptions);
                    
                    // ✅ Cache token expiration for optimization
                    CacheTokenExpiration(result.AccessToken);
                    
                    _logger.LogDebug("JWT token stored in HttpOnly cookie with SameSite: {SameSite}", 
                        _cookieConfig.JwtTokenSameSite);
                }

                // Store refresh token in HttpOnly cookie only
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    var refreshCookieOptions = CreateCookieOptions(
                        isSecure: _cookieConfig.RequireSecure && httpContext.Request.IsHttps,
                        sameSite: _cookieConfig.RefreshTokenSameSite,
                        expires: DateTime.UtcNow.AddDays(30)
                    );
                    
                    response.Cookies.Append("refresh_token", result.RefreshToken, refreshCookieOptions);
                    
                    _logger.LogDebug("Refresh token stored in HttpOnly cookie with SameSite: {SameSite}", 
                        _cookieConfig.RefreshTokenSameSite);
                }

                // Store user information in session (only non-sensitive data)
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
        /// Cache token expiration for optimization
        /// </summary>
        private void CacheTokenExpiration(string token)
        {
            try
            {
                // Parse JWT token to extract expiration
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    _logger.LogWarning("Invalid JWT token format");
                    return;
                }

                // Decode payload (second part)
                var payload = tokenParts[1];
                var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
                var payloadJson = System.Text.Encoding.UTF8.GetString(decodedPayload);
                
                var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson, _jsonOptions);
                
                // Extract expiration
                if (payloadData.TryGetProperty("exp", out var expElement))
                {
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
                    
                    // Cache expiration and token hash for validation
                    _cachedTokenExpiration = expirationTime;
                    _cachedTokenHash = GetTokenHash(token);
                    
                    _logger.LogDebug("Token expiration cached: {Expiration}", expirationTime);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching token expiration");
                // Clear cache on error
                _cachedTokenExpiration = null;
                _cachedTokenHash = null;
            }
        }

        /// <summary>
        /// Get simple hash of token for cache validation
        /// </summary>
        private string GetTokenHash(string token)
        {
            // Simple hash for cache validation - not for security
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(token)));
        }

        /// <summary>
        /// Get cached token expiration if available and valid
        /// </summary>
        public DateTimeOffset? GetCachedTokenExpiration(string currentToken)
        {
            try
            {
                if (_cachedTokenExpiration == null || _cachedTokenHash == null)
                {
                    return null;
                }

                // Validate that the cached expiration is for the current token
                var currentTokenHash = GetTokenHash(currentToken);
                if (currentTokenHash != _cachedTokenHash)
                {
                    _logger.LogDebug("Token hash mismatch, clearing cache");
                    _cachedTokenExpiration = null;
                    _cachedTokenHash = null;
                    return null;
                }

                return _cachedTokenExpiration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached token expiration");
                return null;
            }
        }

        /// <summary>
        /// Clear token expiration cache
        /// </summary>
        private void ClearTokenExpirationCache()
        {
            _cachedTokenExpiration = null;
            _cachedTokenHash = null;
            _logger.LogDebug("Token expiration cache cleared");
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
        /// Clear all user session data and cookies
        /// </summary>
        public async Task ClearUserSessionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                // First, try to invalidate refresh token on backend
                await InvalidateRefreshTokenOnBackendAsync(cancellationToken);

                // Then clear local session and cookies
                var session = httpContext.Session;
                var response = httpContext.Response;

                // Clear session
                session.Clear();

                // Clear cookies
                response.Cookies.Delete("jwt_token");
                response.Cookies.Delete("refresh_token");

                // ✅ Clear token expiration cache
                ClearTokenExpirationCache();

                _logger.LogInformation("User session cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user session");
                throw;
            }
        }

        /// <summary>
        /// Clear user session data (synchronous version for backward compatibility)
        /// </summary>
        public void ClearUserSession()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                var session = httpContext.Session;
                var response = httpContext.Response;

                // Clear session
                session.Clear();

                // Clear cookies
                response.Cookies.Delete("jwt_token");
                response.Cookies.Delete("refresh_token");

                // ✅ Clear token expiration cache
                ClearTokenExpirationCache();

                _logger.LogInformation("User session cleared successfully");
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
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            try
            {
                var jwtToken = GetJwtToken();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return false;
                }

                // Validate JWT token and check expiration
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(jwtToken))
                {
                    _logger.LogWarning("Invalid JWT token format");
                    return false;
                }

                var token = handler.ReadJwtToken(jwtToken);
                var exp = token.ValidTo;

                // Check if token is expired
                if (exp <= DateTime.UtcNow)
                {
                    _logger.LogDebug("JWT token has expired at {ExpirationTime}", exp);
                    return false;
                }

                // Check if user info is available in session
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogDebug("User ID not found in session");
                    return false;
                }

                _logger.LogDebug("User is authenticated. Token expires at {ExpirationTime}", exp);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token");
                return false;
            }
        }

        /// <summary>
        /// Check if JWT token is about to expire (within specified minutes)
        /// </summary>
        /// <param name="minutesBeforeExpiry">Minutes before expiry to consider token as "about to expire"</param>
        /// <returns>True if token expires within specified minutes, false otherwise</returns>
        public bool IsTokenAboutToExpire(int minutesBeforeExpiry = 5)
        {
            try
            {
                var jwtToken = GetJwtToken();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return false;
                }

                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(jwtToken))
                {
                    return false;
                }

                var token = handler.ReadJwtToken(jwtToken);
                var exp = token.ValidTo;
                var threshold = DateTime.UtcNow.AddMinutes(minutesBeforeExpiry);

                return exp <= threshold;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token expiration");
                return false;
            }
        }

        /// <summary>
        /// Get JWT token expiration time
        /// </summary>
        /// <returns>Token expiration time if valid, null otherwise</returns>
        public DateTime? GetTokenExpiration()
        {
            try
            {
                var jwtToken = GetJwtToken();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return null;
                }

                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(jwtToken))
                {
                    return null;
                }

                var token = handler.ReadJwtToken(jwtToken);
                return token.ValidTo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token expiration");
                return null;
            }
        }

        /// <summary>
        /// Check if refresh token exists and is valid
        /// </summary>
        /// <returns>True if refresh token exists, false otherwise</returns>
        public bool HasValidRefreshToken()
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return false;
                }

                // Basic validation: check if token has reasonable length
                // Refresh tokens are typically longer than JWT tokens
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
        /// <returns>Refresh token expiration time if it's a JWT, null otherwise</returns>
        public DateTime? GetRefreshTokenExpiration()
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return null;
                }

                // Try to parse as JWT token first
                var handler = new JwtSecurityTokenHandler();
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
        /// Check if refresh token is about to expire (within specified days)
        /// </summary>
        /// <param name="daysBeforeExpiry">Days before expiry to consider token as "about to expire"</param>
        /// <returns>True if token expires within specified days, false otherwise</returns>
        public bool IsRefreshTokenAboutToExpire(int daysBeforeExpiry = 7)
        {
            try
            {
                var expiration = GetRefreshTokenExpiration();
                if (!expiration.HasValue)
                {
                    // Opaque token, can't check expiration
                    return false;
                }

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
        /// <returns>Token type if valid, null otherwise</returns>
        public string? GetRefreshTokenType()
        {
            try
            {
                var refreshToken = GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return null;
                }

                var handler = new JwtSecurityTokenHandler();
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

                // ✅ Cache new token expiration for optimization
                CacheTokenExpiration(newToken);

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

        /// <summary>
        /// Logout user with options
        /// </summary>
        /// <param name="logoutFromAllDevices">Whether to logout from all devices</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Logout result with success status and details</returns>
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

                // Always clear local session and cookies
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
    }
} 