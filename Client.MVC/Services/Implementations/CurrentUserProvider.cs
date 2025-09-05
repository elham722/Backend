using Client.MVC.Services.Abstractions;

namespace Client.MVC.Services.Implementations
{
    /// <summary>
    /// Implementation of ICurrentUser that provides current authenticated user information
    /// Uses JWT claims to extract user data - stateless approach
    /// </summary>
    public class CurrentUserProvider : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtClaimsExtractor _jwtClaimsExtractor;
        private readonly ILogger<CurrentUserProvider> _logger;

        public CurrentUserProvider(
            IHttpContextAccessor httpContextAccessor,
            IJwtClaimsExtractor jwtClaimsExtractor,
            ILogger<CurrentUserProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _jwtClaimsExtractor = jwtClaimsExtractor ?? throw new ArgumentNullException(nameof(jwtClaimsExtractor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get current user ID from JWT claims
        /// </summary>
        public string? GetUserId()
        {
            try
            {
                var jwtToken = GetJwtTokenFromCookie();
                return _jwtClaimsExtractor.GetUserId(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user ID from JWT token");
                return null;
            }
        }

        /// <summary>
        /// Get current user name from JWT claims
        /// </summary>
        public string? GetUserName()
        {
            try
            {
                var jwtToken = GetJwtTokenFromCookie();
                return _jwtClaimsExtractor.GetUserName(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user name from JWT token");
                return null;
            }
        }

        /// <summary>
        /// Get current user email from JWT claims
        /// </summary>
        public string? GetUserEmail()
        {
            try
            {
                var jwtToken = GetJwtTokenFromCookie();
                return _jwtClaimsExtractor.GetUserEmail(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user email from JWT token");
                return null;
            }
        }

        /// <summary>
        /// Get current user roles from JWT claims
        /// </summary>
        public IEnumerable<string> GetUserRoles()
        {
            try
            {
                var jwtToken = GetJwtTokenFromCookie();
                return _jwtClaimsExtractor.GetUserRoles(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user roles from JWT token");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Check if user is currently authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            try
            {
                var jwtToken = GetJwtTokenFromCookie();
                return !string.IsNullOrEmpty(jwtToken) && _jwtClaimsExtractor.IsTokenValid(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check authentication status");
                return false;
            }
        }

        /// <summary>
        /// Get JWT token from HTTP-only cookie
        /// </summary>
        private string? GetJwtTokenFromCookie()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Cookies != null)
            {
                return httpContext.Request.Cookies["JwtToken"];
            }
            return null;
        }
    }
}