using Backend.Application.Features.UserManagement.DTOs;
using Client.MVC.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Client.MVC.Services.ApiClients
{
    /// <summary>
    /// Implementation of authentication client for background jobs and external services
    /// </summary>
    public class BackgroundJobAuthClient : IBackgroundJobAuthClient
    {
        private readonly IAuthenticatedHttpClient _httpClient;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<BackgroundJobAuthClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public BackgroundJobAuthClient(
            IAuthenticatedHttpClient httpClient,
            IUserSessionService userSessionService,
            ILogger<BackgroundJobAuthClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Validate a JWT token locally (without API call)
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token validation failed: token is null or empty");
                    return false;
                }

                _logger.LogDebug("Validating token locally");
                
                // Parse JWT token to check expiration
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    _logger.LogWarning("Invalid JWT token format");
                    return false;
                }

                // Decode payload (second part)
                var payload = tokenParts[1];
                var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
                var payloadJson = Encoding.UTF8.GetString(decodedPayload);
                
                var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson, _jsonOptions);
                
                // Check expiration
                if (payloadData.TryGetProperty("exp", out var expElement))
                {
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
                    var currentTime = DateTimeOffset.UtcNow;
                    
                    // Token is valid if it expires in more than 5 minutes
                    var isValid = expirationTime > currentTime.AddMinutes(5);
                    
                    if (isValid)
                    {
                        _logger.LogDebug("Token validation successful locally. Expires: {Expiration}", expirationTime);
                    }
                    else
                    {
                        _logger.LogDebug("Token is expired or expiring soon. Expires: {Expiration}, Current: {Current}", 
                            expirationTime, currentTime);
                    }
                    
                    return isValid;
                }

                _logger.LogWarning("Token does not contain expiration claim");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during local token validation");
                return false;
                // Note: We're using async Task<bool> for interface compatibility
                // but this method is actually synchronous
            }
        }

        /// <summary>
        /// Validate current user's token from session/cookies
        /// </summary>
        public async Task<bool> ValidateCurrentTokenAsync()
        {
            try
            {
                var currentToken = _userSessionService.GetJwtToken();
                if (string.IsNullOrEmpty(currentToken))
                {
                    _logger.LogDebug("No current token found for validation");
                    return false;
                }

                return await ValidateTokenAsync(currentToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating current token");
                return false;
            }
        }

        /// <summary>
        /// Get token information without validation (for logging/debugging)
        /// </summary>
        public Task<object?> GetTokenInfoAsync(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return Task.FromResult<object?>(null);
                }

                // Parse JWT token to extract information
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    _logger.LogWarning("Invalid JWT token format");
                    return Task.FromResult<object?>(null);
                }

                // Decode payload (second part)
                var payload = tokenParts[1];
                var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
                var payloadJson = Encoding.UTF8.GetString(decodedPayload);
                
                var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson, _jsonOptions);
                
                // Extract useful information
                var tokenInfo = new
                {
                    IssuedAt = GetClaimValue(payloadData, "iat"),
                    ExpiresAt = GetClaimValue(payloadData, "exp"),
                    ExpiresAtReadable = GetReadableTimestamp(GetClaimValue(payloadData, "exp")),
                    Subject = GetClaimValue(payloadData, "sub"),
                    UserName = GetClaimValue(payloadData, "name"),
                    Email = GetClaimValue(payloadData, "email"),
                    Roles = GetClaimValues(payloadData, "role"),
                    // Add more useful claims
                    UserId = GetClaimValue(payloadData, "UserId"),
                    EmailConfirmed = GetClaimValue(payloadData, "EmailConfirmed"),
                    IsActive = GetClaimValue(payloadData, "IsActive"),
                    PhoneNumber = GetClaimValue(payloadData, "PhoneNumber")
                };

                return Task.FromResult<object?>(tokenInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token information");
                return Task.FromResult<object?>(null);
            }
        }

        private static string? GetClaimValue(JsonElement payload, string claimName)
        {
            if (payload.TryGetProperty(claimName, out var element))
            {
                return element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number => element.GetInt64().ToString(),
                    _ => element.ToString()
                };
            }
            return null;
        }

        private static List<string> GetClaimValues(JsonElement payload, string claimName)
        {
            var values = new List<string>();
            
            if (payload.TryGetProperty(claimName, out var element))
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        values.Add(item.ToString());
                    }
                }
                else
                {
                    values.Add(element.ToString());
                }
            }
            
            return values;
        }

        private static string? GetReadableTimestamp(string? timestamp)
        {
            if (string.IsNullOrEmpty(timestamp) || !long.TryParse(timestamp, out var unixTimestamp))
            {
                return null;
            }

            try
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
            }
            catch
            {
                return timestamp;
            }
        }
    }
} 