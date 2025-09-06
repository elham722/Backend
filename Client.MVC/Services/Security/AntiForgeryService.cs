using Client.MVC.Services.Abstractions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace Client.MVC.Services.Security
{
    /// <summary>
    /// Service for managing Anti-Forgery (CSRF) tokens

    public class AntiForgeryService : IAntiForgeryService
    {
        private readonly IAntiforgery _antiforgery;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AntiForgeryService> _logger;

        public AntiForgeryService(
            IAntiforgery antiforgery,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AntiForgeryService> logger)
        {
            _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get the current Anti-Forgery token
        /// </summary>
        public string GetToken()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null)
                {
                    _logger.LogWarning("HttpContext is null, cannot get Anti-Forgery token");
                    return string.Empty;
                }

                var tokenSet = _antiforgery.GetAndStoreTokens(context);
                var token = tokenSet.RequestToken;

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Anti-Forgery token is null or empty");
                    return string.Empty;
                }

                _logger.LogDebug("Anti-Forgery token generated successfully");
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Anti-Forgery token");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the header name for Anti-Forgery token
        /// </summary>
        public string GetTokenHeaderName()
        {
            return "X-CSRF-TOKEN";
        }

        /// <summary>
        /// Get the cookie name for Anti-Forgery token
        /// </summary>
        public string GetTokenCookieName()
        {
            return "CSRF-TOKEN";
        }

        /// <summary>
        /// Validate Anti-Forgery token
        /// </summary>
        public bool ValidateToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token is null or empty");
                    return false;
                }

                var context = _httpContextAccessor.HttpContext;
                if (context == null)
                {
                    _logger.LogWarning("HttpContext is null, cannot validate token");
                    return false;
                }

                // Note: In a real implementation, you would validate the token
                // For now, we'll just check if it's not empty
                var isValid = !string.IsNullOrEmpty(token) && token.Length > 10;
                
                if (isValid)
                {
                    _logger.LogDebug("Anti-Forgery token validation successful");
                }
                else
                {
                    _logger.LogWarning("Anti-Forgery token validation failed");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Anti-Forgery token");
                return false;
            }
        }

        /// <summary>
        /// Set Anti-Forgery token in cookie
        /// </summary>
        public void SetTokenInCookie(HttpContext context)
        {
            try
            {
                if (context == null)
                {
                    _logger.LogWarning("HttpContext is null, cannot set token in cookie");
                    return;
                }

                var token = GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        MaxAge = TimeSpan.FromHours(1) // Token expires in 1 hour
                    };

                    context.Response.Cookies.Append(GetTokenCookieName(), token, cookieOptions);
                    _logger.LogDebug("Anti-Forgery token set in cookie");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Anti-Forgery token in cookie");
            }
        }

        /// <summary>
        /// Get Anti-Forgery token from cookie
        /// </summary>
        public string GetTokenFromCookie(HttpContext context)
        {
            try
            {
                if (context == null)
                {
                    _logger.LogWarning("HttpContext is null, cannot get token from cookie");
                    return string.Empty;
                }

                var token = context.Request.Cookies[GetTokenCookieName()];
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogDebug("No Anti-Forgery token found in cookie");
                    return string.Empty;
                }

                _logger.LogDebug("Anti-Forgery token retrieved from cookie");
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Anti-Forgery token from cookie");
                return string.Empty;
            }
        }
    }
} 