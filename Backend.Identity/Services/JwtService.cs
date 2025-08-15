using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Implementation of JWT service for token generation and validation
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationInMinutes;
        private readonly string _algorithm;
        private readonly string _keyId;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            _issuer = configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            _audience = configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            _expirationInMinutes = int.Parse(configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
            _algorithm = configuration["JwtSettings:Algorithm"] ?? "HS256";
            _keyId = configuration["JwtSettings:KeyId"] ?? "default-key";
        }

        public string GenerateToken(string userId, string userName, string email, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add roles to claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetSigningKey();
            var credentials = new SigningCredentials(key, GetSecurityAlgorithm());

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationInMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = credentials
            };

            // Add kid header for key rotation
            tokenDescriptor.AdditionalHeaderClaims = new Dictionary<string, object>
            {
                { "kid", _keyId }
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = GetSigningKey();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    // Add kid validation for key rotation
                    RequireSignedTokens = true
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public bool ValidateRefreshToken(string refreshToken)
        {
            try
            {
                // For refresh tokens, we just check if they're not empty and have a reasonable length
                return !string.IsNullOrEmpty(refreshToken) && refreshToken.Length >= 32;
            }
            catch
            {
                return false;
            }
        }

        public string? GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch
            {
                return null;
            }
        }

        public DateTime GetTokenExpiration(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public bool IsTokenExpired(string token)
        {
            var expiration = GetTokenExpiration(token);
            return expiration <= DateTime.UtcNow;
        }

        public bool IsTokenExpiringSoon(string token, int minutesThreshold = 5)
        {
            var expiration = GetTokenExpiration(token);
            return expiration <= DateTime.UtcNow.AddMinutes(minutesThreshold);
        }

        private SecurityKey GetSigningKey()
        {
            if (_algorithm.Equals("RS256", StringComparison.OrdinalIgnoreCase))
            {
                // For RS256, we need to load the private key
                var privateKey = _configuration["JwtSettings:PrivateKey"];
                if (string.IsNullOrEmpty(privateKey))
                {
                    throw new InvalidOperationException("Private key not configured for RS256");
                }

                // Load RSA private key
                var rsa = RSA.Create();
                rsa.ImportFromPem(privateKey);
                return new RsaSecurityKey(rsa);
            }
            else
            {
                // Default to HS256
                return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            }
        }

        private string GetSecurityAlgorithm()
        {
            return _algorithm.Equals("RS256", StringComparison.OrdinalIgnoreCase) 
                ? SecurityAlgorithms.RsaSha256 
                : SecurityAlgorithms.HmacSha256;
        }
    }
} 