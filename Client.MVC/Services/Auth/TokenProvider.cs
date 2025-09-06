using Client.MVC.Services.Abstractions;
using Client.MVC.Services.Security;
using System.Collections.Concurrent;

namespace Client.MVC.Services.Auth
{
    /// <summary>
    /// Implementation of ITokenProvider that handles JWT and refresh token operations
    /// Uses secure cookies for token storage with caching for performance
    /// </summary>
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtClaimsExtractor _jwtClaimsExtractor;
        private readonly ILogger<TokenProvider> _logger;
        private readonly CookieSecurityConfig _cookieConfig;
        
        // Cache for token expiration times to avoid repeated JWT parsing
        private static readonly ConcurrentDictionary<string, DateTime> _tokenExpirationCache = new();

        public TokenProvider(
            IHttpContextAccessor httpContextAccessor,
            IJwtClaimsExtractor jwtClaimsExtractor,
            ILogger<TokenProvider> logger,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _jwtClaimsExtractor = jwtClaimsExtractor ?? throw new ArgumentNullException(nameof(jwtClaimsExtractor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cookieConfig = CookieSecurityConfig.FromConfiguration(configuration);
        }

        /// <summary>
        /// Get JWT token from secure HTTP-only cookie
        /// </summary>
        public string? GetJwtToken()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Request.Cookies["JwtToken"];
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get JWT token from cookie");
                return null;
            }
        }

        /// <summary>
        /// Get refresh token from secure HTTP-only cookie
        /// </summary>
        public string? GetRefreshToken()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Request.Cookies["RefreshToken"];
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get refresh token from cookie");
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
                if (expiration == null)
                    return true; // Consider as expired if we can't get expiration

                return DateTime.UtcNow.AddMinutes(minutesBeforeExpiry) >= expiration.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check token expiration");
                return true; // Consider as expired on error
            }
        }

        /// <summary>
        /// Get JWT token expiration time
        /// </summary>
        public DateTime? GetTokenExpiration()
        {
            try
            {
                var jwtToken = GetJwtToken();
                if (string.IsNullOrEmpty(jwtToken))
                    return null;

                // Check cache first
                var cachedExpiration = GetCachedTokenExpiration(jwtToken);
                if (cachedExpiration.HasValue)
                    return cachedExpiration.Value.DateTime;

                // Extract from JWT
                var expiration = _jwtClaimsExtractor.GetTokenExpiration(jwtToken);
                
                // Cache the result
                if (expiration.HasValue)
                {
                    _tokenExpirationCache.TryAdd(jwtToken, expiration.Value);
                }

                return expiration;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get token expiration");
                return null;
            }
        }

        /// <summary>
        /// Get cached token expiration if available and valid
        /// </summary>
        public DateTimeOffset? GetCachedTokenExpiration(string currentToken)
        {
            if (string.IsNullOrEmpty(currentToken))
                return null;

            try
            {
                if (_tokenExpirationCache.TryGetValue(currentToken, out var cachedExpiration))
                {
                    // Verify cache is still valid (not expired)
                    if (cachedExpiration > DateTime.UtcNow)
                    {
                        return new DateTimeOffset(cachedExpiration);
                    }
                    else
                    {
                        // Remove expired cache entry
                        _tokenExpirationCache.TryRemove(currentToken, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get cached token expiration");
            }

            return null;
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

                // If it's a JWT refresh token, check expiration
                if (_jwtClaimsExtractor.IsTokenValid(refreshToken))
                {
                    var expiration = _jwtClaimsExtractor.GetTokenExpiration(refreshToken);
                    return expiration.HasValue && expiration.Value > DateTimeOffset.UtcNow;
                }

                // For opaque refresh tokens, we assume they're valid if they exist
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate refresh token");
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

                // Try to parse as JWT
                var expiration = _jwtClaimsExtractor.GetTokenExpiration(refreshToken);
                return expiration;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get refresh token expiration");
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
                if (expiration == null)
                    return false; // Opaque tokens don't have expiration we can check

                return DateTime.UtcNow.AddDays(daysBeforeExpiry) >= expiration.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check refresh token expiration");
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

                // Check if it's a valid JWT
                return _jwtClaimsExtractor.IsTokenValid(refreshToken) ? "JWT" : "Opaque";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to determine refresh token type");
                return null;
            }
        }

        /// <summary>
        /// Refresh JWT token (called after token refresh)
        /// </summary>
        public void RefreshJwtToken(string newToken, DateTime? expiresAt = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogError("HttpContext is null, cannot refresh JWT token");
                    return;
                }

                // Remove old token from cache
                var oldToken = GetJwtToken();
                if (!string.IsNullOrEmpty(oldToken))
                {
                    _tokenExpirationCache.TryRemove(oldToken, out _);
                }

                // Set new token in cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = _cookieConfig.RequireSecure,
                    SameSite = _cookieConfig.JwtTokenSameSite,
                    Path = _cookieConfig.Path,
                    Expires = expiresAt
                };

                if (!string.IsNullOrEmpty(_cookieConfig.Domain))
                {
                    cookieOptions.Domain = _cookieConfig.Domain;
                }

                httpContext.Response.Cookies.Append("JwtToken", newToken, cookieOptions);

                // Cache new token expiration
                if (expiresAt.HasValue)
                {
                    _tokenExpirationCache.TryAdd(newToken, expiresAt.Value);
                }

                _logger.LogInformation("JWT token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh JWT token");
                throw;
            }
        }
    }
}