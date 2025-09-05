using Client.MVC.Services.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Client.MVC.Services.Handler
{
    /// <summary>
    /// Custom authentication handler for JWT tokens stored in cookies
    /// </summary>
    public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserSessionService _userSessionService;

        public JwtAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserSessionService userSessionService)
            : base(options, logger, encoder, clock)
        {
            _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Get JWT token from cookies or session
                var jwtToken = _userSessionService.GetJwtToken();
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return AuthenticateResult.NoResult();
                }

                // Validate token locally
                if (!IsTokenValid(jwtToken))
                {
                    return AuthenticateResult.Fail("Token is expired or invalid");
                }

                // Extract claims from JWT token
                var claims = ExtractClaimsFromToken(jwtToken);
                if (claims == null || !claims.Any())
                {
                    return AuthenticateResult.Fail("Unable to extract claims from token");
                }

                // Create claims identity and principal
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during JWT authentication");
                return AuthenticateResult.Fail("Authentication error occurred");
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // Redirect to login page
            Response.Redirect("/Auth/Login");
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            // Redirect to access denied page
            Response.Redirect("/Auth/AccessDenied");
        }

        private bool IsTokenValid(string token)
        {
            try
            {
                // Parse JWT token to check expiration
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    return false;
                }

                // Decode payload (second part)
                var payload = tokenParts[1];
                var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
                var payloadJson = System.Text.Encoding.UTF8.GetString(decodedPayload);
                
                // Simple JSON parsing to get expiration
                if (payloadJson.Contains("\"exp\":"))
                {
                    var expStart = payloadJson.IndexOf("\"exp\":") + 6;
                    var expEnd = payloadJson.IndexOf(",", expStart);
                    if (expEnd == -1) expEnd = payloadJson.IndexOf("}", expStart);
                    
                    if (expEnd > expStart)
                    {
                        var expValue = payloadJson.Substring(expStart, expEnd - expStart).Trim();
                        if (long.TryParse(expValue, out var expirationTime))
                        {
                            var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(expirationTime);
                            var currentTime = DateTimeOffset.UtcNow;
                            
                            // Token is valid if it expires in more than 5 minutes
                            return expirationDateTime > currentTime.AddMinutes(5);
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private List<Claim> ExtractClaimsFromToken(string token)
        {
            try
            {
                var claims = new List<Claim>();
                
                // Parse JWT token
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    return claims;
                }

                // Decode payload (second part)
                var payload = tokenParts[1];
                var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
                var payloadJson = System.Text.Encoding.UTF8.GetString(decodedPayload);
                
                // Extract common claims
                var userId = ExtractClaimValue(payloadJson, "UserId");
                var userName = ExtractClaimValue(payloadJson, "name");
                var email = ExtractClaimValue(payloadJson, "email");
                var roles = ExtractClaimValues(payloadJson, "role");

                // Add claims
                if (!string.IsNullOrEmpty(userId))
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                
                if (!string.IsNullOrEmpty(userName))
                    claims.Add(new Claim(ClaimTypes.Name, userName));
                
                if (!string.IsNullOrEmpty(email))
                    claims.Add(new Claim(ClaimTypes.Email, email));

                // Add roles
                foreach (var role in roles)
                {
                    if (!string.IsNullOrEmpty(role))
                        claims.Add(new Claim(ClaimTypes.Role, role));
                }

                return claims;
            }
            catch
            {
                return new List<Claim>();
            }
        }

        private string? ExtractClaimValue(string json, string claimName)
        {
            try
            {
                var pattern = $"\"{claimName}\":\"";
                var startIndex = json.IndexOf(pattern);
                if (startIndex == -1) return null;

                startIndex += pattern.Length;
                var endIndex = json.IndexOf("\"", startIndex);
                if (endIndex == -1) return null;

                return json.Substring(startIndex, endIndex - startIndex);
            }
            catch
            {
                return null;
            }
        }

        private List<string> ExtractClaimValues(string json, string claimName)
        {
            var values = new List<string>();
            try
            {
                // Handle array claims like roles
                var pattern = $"\"{claimName}\":";
                var startIndex = json.IndexOf(pattern);
                if (startIndex == -1) return values;

                startIndex += pattern.Length;
                
                // Check if it's an array
                if (json[startIndex] == '[')
                {
                    startIndex++; // Skip '['
                    var endIndex = json.IndexOf("]", startIndex);
                    if (endIndex > startIndex)
                    {
                        var arrayContent = json.Substring(startIndex, endIndex - startIndex);
                        var items = arrayContent.Split(',');
                        foreach (var item in items)
                        {
                            var cleanItem = item.Trim().Trim('"');
                            if (!string.IsNullOrEmpty(cleanItem))
                                values.Add(cleanItem);
                        }
                    }
                }
                else
                {
                    // Single value
                    var value = ExtractClaimValue(json, claimName);
                    if (!string.IsNullOrEmpty(value))
                        values.Add(value);
                }
            }
            catch
            {
                // Return empty list on error
            }

            return values;
        }
    }
} 