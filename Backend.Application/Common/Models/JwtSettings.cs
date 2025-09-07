namespace Backend.Application.Common.Models
{
    /// <summary>
    /// Strongly typed JWT settings configuration
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "Jwt";
        
        public string SecretKey { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int AccessTokenExpirationMinutes { get; set; } = 15;
        public int RefreshTokenExpirationDays { get; set; } = 30;
        public bool RequireHttps { get; set; } = true;
        public bool ValidateIssuer { get; set; } = true;
        public bool ValidateAudience { get; set; } = true;
        public bool ValidateLifetime { get; set; } = true;
        public bool ValidateIssuerSigningKey { get; set; } = true;
        public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;
        public bool RequireExpirationTime { get; set; } = true;
        public bool RequireSignedTokens { get; set; } = true;
    }
}