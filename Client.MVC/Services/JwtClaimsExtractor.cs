using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Client.MVC.Services
{
    /// <summary>
    /// Service for extracting claims from JWT tokens
    /// </summary>
    public interface IJwtClaimsExtractor
    {
        string? GetUserId(string? jwtToken);
        string? GetUserName(string? jwtToken);
        string? GetUserEmail(string? jwtToken);
        IEnumerable<string> GetUserRoles(string? jwtToken);
        DateTime? GetTokenExpiration(string? jwtToken);
        bool IsTokenValid(string? jwtToken);
        IDictionary<string, object> GetAllClaims(string? jwtToken);
        
        // ✅ New methods for generic claim extraction
        string? GetClaimValue(string claimType, string? jwtToken = null);
        IEnumerable<string> GetClaimValues(string claimType, string? jwtToken = null);
    }

    public class JwtClaimsExtractor : IJwtClaimsExtractor
    {
        private readonly ILogger<JwtClaimsExtractor> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public JwtClaimsExtractor(ILogger<JwtClaimsExtractor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Get user ID from JWT token
        /// </summary>
        public string? GetUserId(string? jwtToken)
        {
            try
            {
                if (string.IsNullOrEmpty(jwtToken))
                    return null;

                var claims = ExtractClaims(jwtToken);
                return claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "user_id")?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user ID from JWT token");
                return null;
            }
        }

        /// <summary>
        /// Get user name from JWT token
        /// </summary>
        public string? GetUserName(string? jwtToken)
        {
            try
            {
                if (string.IsNullOrEmpty(jwtToken))
                    return null;

                var claims = ExtractClaims(jwtToken);
                return claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name" || c.Type == "username")?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user name from JWT token");
                return null;
            }
        }

        /// <summary>
        /// Get user email from JWT token
        /// </summary>
        public string? GetUserEmail(string? jwtToken)
        {
            try
            {
                if (string.IsNullOrEmpty(jwtToken))
                    return null;

                var claims = ExtractClaims(jwtToken);
                return claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user email from JWT token");
                return null;
            }
        }

        /// <summary>
        /// Get user roles from JWT token
        /// </summary>
        public IEnumerable<string> GetUserRoles(string? jwtToken)
        {
            try
            {
                if (string.IsNullOrEmpty(jwtToken))
                    return Enumerable.Empty<string>();

                var claims = ExtractClaims(jwtToken);
                return claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
                    .Select(c => c.Value)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user roles from JWT token");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Get token expiration from JWT token
        /// </summary>
        public DateTime? GetTokenExpiration(string? jwtToken)
        {
            try
            {
                if (string.IsNullOrEmpty(jwtToken))
                    return null;

                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(jwtToken))
                    return null;

                var token = handler.ReadJwtToken(jwtToken);
                return token.ValidTo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting token expiration from JWT token");
                return null;
            }
        }

        /// <summary>
        /// Check if JWT token is valid
        /// </summary>
        public bool IsTokenValid(string? jwtToken)
        {
            try
            {
                if (string.IsNullOrEmpty(jwtToken))
                    return false;

                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(jwtToken))
                    return false;

                var token = handler.ReadJwtToken(jwtToken);
                
                // Check if token is expired
                if (token.ValidTo <= DateTime.UtcNow)
                    return false;

                // Check if token has required claims
                var userId = GetUserId(jwtToken);
                if (string.IsNullOrEmpty(userId))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token");
                return false;
            }
        }

        /// <summary>
        /// Get all claims from JWT token as dictionary
        /// </summary>
        public IDictionary<string, object> GetAllClaims(string? jwtToken)
        {
            try
            {
                if (string.IsNullOrEmpty(jwtToken))
                    return new Dictionary<string, object>();

                var claims = ExtractClaims(jwtToken);
                return claims.ToDictionary(c => c.Type, c => (object)c.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting all claims from JWT token");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// ✅ Get a specific claim value by claim type
        /// </summary>
        public string? GetClaimValue(string claimType, string? jwtToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(claimType))
                    return null;

                // If no JWT token provided, try to get from current context
                if (string.IsNullOrEmpty(jwtToken))
                {
                    // This method is designed to work with a provided JWT token
                    // For context-based extraction, use the specific methods like GetUserId()
                    return null;
                }

                var claims = ExtractClaims(jwtToken);
                return claims.FirstOrDefault(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting claim value for type {ClaimType}", claimType);
                return null;
            }
        }

        /// <summary>
        /// ✅ Get multiple claim values by claim type (for array claims like roles)
        /// </summary>
        public IEnumerable<string> GetClaimValues(string claimType, string? jwtToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(claimType))
                    return Enumerable.Empty<string>();

                // If no JWT token provided, try to get from current context
                if (string.IsNullOrEmpty(jwtToken))
                {
                    // This method is designed to work with a provided JWT token
                    // For context-based extraction, use the specific methods like GetUserRoles()
                    return Enumerable.Empty<string>();
                }

                var claims = ExtractClaims(jwtToken);
                return claims
                    .Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Value)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting claim values for type {ClaimType}", claimType);
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Extract claims from JWT token
        /// </summary>
        private IEnumerable<Claim> ExtractClaims(string jwtToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(jwtToken))
                    return Enumerable.Empty<Claim>();

                var token = handler.ReadJwtToken(jwtToken);
                return token.Claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting claims from JWT token");
                return Enumerable.Empty<Claim>();
            }
        }
    }
} 