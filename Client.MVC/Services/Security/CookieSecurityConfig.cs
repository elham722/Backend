using Microsoft.AspNetCore.Http;

namespace Client.MVC.Services.Security
{
    /// <summary>
    /// Configuration for cookie security settings
    /// Used to configure secure cookie options for authentication tokens
    /// </summary>
    public class CookieSecurityConfig
    {
        /// <summary>
        /// SameSite mode for JWT token cookies
        /// </summary>
        public SameSiteMode JwtTokenSameSite { get; set; } = SameSiteMode.Strict;

        /// <summary>
        /// SameSite mode for refresh token cookies
        /// </summary>
        public SameSiteMode RefreshTokenSameSite { get; set; } = SameSiteMode.Lax;

        /// <summary>
        /// Whether cookies require HTTPS (secure flag)
        /// </summary>
        public bool RequireSecure { get; set; } = true;

        /// <summary>
        /// Cookie domain (optional)
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// Cookie path
        /// </summary>
        public string Path { get; set; } = "/";

        /// <summary>
        /// Cookie expiration time in minutes for JWT tokens
        /// </summary>
        public int ExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Cookie expiration time in days for refresh tokens
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 30;

        /// <summary>
        /// Create configuration from IConfiguration
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <returns>Configured CookieSecurityConfig instance</returns>
        public static CookieSecurityConfig FromConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetSection("CookieSecurity");
            
            return new CookieSecurityConfig
            {
                JwtTokenSameSite = ParseSameSite(section["JwtTokenSameSite"], SameSiteMode.Strict),
                RefreshTokenSameSite = ParseSameSite(section["RefreshTokenSameSite"], SameSiteMode.Lax),
                RequireSecure = section.GetValue("RequireSecure", true),
                Domain = section["Domain"],
                Path = section["Path"] ?? "/",
                ExpirationMinutes = section.GetValue("ExpirationMinutes", 30),
                RefreshTokenExpirationDays = section.GetValue("RefreshTokenExpirationDays", 30)
            };
        }

        /// <summary>
        /// Parse SameSite mode from string value
        /// </summary>
        /// <param name="value">String value to parse</param>
        /// <param name="defaultValue">Default value if parsing fails</param>
        /// <returns>Parsed SameSiteMode</returns>
        private static SameSiteMode ParseSameSite(string? value, SameSiteMode defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value.ToLowerInvariant() switch
            {
                "strict" => SameSiteMode.Strict,
                "lax" => SameSiteMode.Lax,
                "none" => SameSiteMode.None,
                "unspecified" => SameSiteMode.Unspecified,
                _ => defaultValue
            };
        }

        /// <summary>
        /// Create secure cookie options for JWT tokens
        /// </summary>
        /// <param name="expiresAt">Optional expiration time</param>
        /// <returns>Configured CookieOptions</returns>
        public CookieOptions CreateJwtCookieOptions(DateTime? expiresAt = null)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = RequireSecure,
                SameSite = JwtTokenSameSite,
                Path = Path,
                Expires = expiresAt ?? DateTime.UtcNow.AddMinutes(ExpirationMinutes)
            };

            if (!string.IsNullOrEmpty(Domain))
            {
                options.Domain = Domain;
            }

            return options;
        }

        /// <summary>
        /// Create secure cookie options for refresh tokens
        /// </summary>
        /// <param name="expiresAt">Optional expiration time</param>
        /// <returns>Configured CookieOptions</returns>
        public CookieOptions CreateRefreshCookieOptions(DateTime? expiresAt = null)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = RequireSecure,
                SameSite = RefreshTokenSameSite,
                Path = Path,
                Expires = expiresAt ?? DateTime.UtcNow.AddDays(RefreshTokenExpirationDays)
            };

            if (!string.IsNullOrEmpty(Domain))
            {
                options.Domain = Domain;
            }

            return options;
        }

        /// <summary>
        /// Create cookie options for deleting cookies
        /// </summary>
        /// <returns>CookieOptions configured for deletion</returns>
        public CookieOptions CreateDeleteCookieOptions()
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = RequireSecure,
                Path = Path,
                Expires = DateTime.UtcNow.AddDays(-1) // Set to past date to delete
            };

            if (!string.IsNullOrEmpty(Domain))
            {
                options.Domain = Domain;
            }

            return options;
        }
    }
}