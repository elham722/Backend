using Backend.Application.Features.UserManagement.DTOs;
using System.Text;
using System.Text.Json;

namespace Client.MVC.Services
{
    /// <summary>
    /// Advanced token manager with concurrency-safe refresh operations
    /// </summary>
    public interface ITokenManager
    {
        Task<bool> RefreshTokenAsync();
        Task<bool> RefreshTokenIfNeededAsync();
        bool IsTokenValid();
        bool IsTokenExpiringSoon(TimeSpan? threshold = null);
        string? GetCurrentToken();
        Task<bool> ValidateTokenAsync();
        void InvalidateToken();
        Task<DateTimeOffset?> GetTokenExpirationAsync();
    }

    public class TokenManager : ITokenManager
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenManager> _logger;
        private readonly IConcurrencyManager _concurrencyManager;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ResiliencePolicyService _resiliencePolicyService;

        private const string REFRESH_OPERATION_KEY = "token-refresh";
        private static readonly TimeSpan DEFAULT_EXPIRATION_THRESHOLD = TimeSpan.FromMinutes(5);

        public TokenManager(
            IUserSessionService userSessionService,
            IHttpClientFactory httpClientFactory,
            ILogger<TokenManager> logger,
            IConcurrencyManager concurrencyManager,
            ResiliencePolicyService resiliencePolicyService)
        {
            _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _concurrencyManager = concurrencyManager ?? throw new ArgumentNullException(nameof(concurrencyManager));
            _resiliencePolicyService = resiliencePolicyService ?? throw new ArgumentNullException(nameof(resiliencePolicyService));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Refresh token with concurrency-safe execution
        /// </summary>
        public async Task<bool> RefreshTokenAsync()
        {
            return await _concurrencyManager.ExecuteWithLockAsync(REFRESH_OPERATION_KEY, async () =>
            {
                _logger.LogInformation("Starting token refresh operation");
                
                var refreshToken = _userSessionService.GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("No refresh token available");
                    return false;
                }

                try
                {
                    // Create refresh token request
                    var request = new RefreshTokenDto { RefreshToken = refreshToken };
                    var json = JsonSerializer.Serialize(request, _jsonOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    // Use critical resilience policy for refresh token endpoint
                    var httpClient = _httpClientFactory.CreateClient("ApiClient");
                    var policy = _resiliencePolicyService.CreateCriticalAuthPolicy();
                    
                    var response = await policy.ExecuteAsync(async (context) =>
                    {
                        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/Auth/refresh-token")
                        {
                            Content = content
                        };
                        
                        _logger.LogDebug("Sending refresh token request");
                        return await httpClient.SendAsync(httpRequest);
                    }, new Polly.Context("token-refresh"));
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonSerializer.Deserialize<AuthResultDto>(responseContent, _jsonOptions);
                        
                        if (result?.IsSuccess == true && !string.IsNullOrEmpty(result.AccessToken))
                        {
                            // Update both JWT token and refresh token in session
                            _userSessionService.SetUserSession(result);
                            _logger.LogInformation("Token refreshed successfully");
                            return true;
                        }
                        else
                        {
                            _logger.LogWarning("Token refresh failed: {Error}", result?.ErrorMessage);
                            return false;
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Token refresh failed with status {StatusCode}: {Error}", 
                            response.StatusCode, errorContent);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing token");
                    return false;
                }
            }, TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Refresh token if needed (expired or expiring soon)
        /// </summary>
        public async Task<bool> RefreshTokenIfNeededAsync()
        {
            if (IsTokenValid() && !IsTokenExpiringSoon())
            {
                _logger.LogDebug("Token is valid and not expiring soon, no refresh needed");
                return true;
            }

            _logger.LogInformation("Token needs refresh - expired or expiring soon");
            return await RefreshTokenAsync();
        }

        /// <summary>
        /// Check if token is valid
        /// </summary>
        public bool IsTokenValid()
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                // Use cached token expiration for optimization
                var cachedExpiration = _userSessionService.GetCachedTokenExpiration(token);
                if (cachedExpiration.HasValue)
                {
                    var currentTime = DateTimeOffset.UtcNow;
                    var isValid = cachedExpiration.Value > currentTime;
                    
                    if (!isValid)
                    {
                        _logger.LogDebug("Token is expired (cached). Expires: {Expiration}, Current: {Current}", 
                            cachedExpiration.Value, currentTime);
                    }
                    
                    return isValid;
                }

                // Fallback to manual decoding
                _logger.LogDebug("No cached expiration available, falling back to manual decoding");
                
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    return false;
                }

                // Decode payload
                var payload = tokenParts[1];
                var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
                var payloadJson = Encoding.UTF8.GetString(decodedPayload);
                
                var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson, _jsonOptions);
                
                if (payloadData.TryGetProperty("exp", out var expElement))
                {
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
                    var currentTime = DateTimeOffset.UtcNow;
                    var isValid = expirationTime > currentTime;
                    
                    if (!isValid)
                    {
                        _logger.LogDebug("Token is expired (manual decode). Expires: {Expiration}, Current: {Current}", 
                            expirationTime, currentTime);
                    }
                    
                    return isValid;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        /// <summary>
        /// Check if token is expiring soon
        /// </summary>
        public bool IsTokenExpiringSoon(TimeSpan? threshold = null)
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return true; // Consider missing token as expiring soon
                }

                var expirationThreshold = threshold ?? DEFAULT_EXPIRATION_THRESHOLD;
                
                // Use cached token expiration for optimization
                var cachedExpiration = _userSessionService.GetCachedTokenExpiration(token);
                if (cachedExpiration.HasValue)
                {
                    var currentTime = DateTimeOffset.UtcNow;
                    var isExpiringSoon = cachedExpiration.Value <= currentTime.Add(expirationThreshold);
                    
                    if (isExpiringSoon)
                    {
                        _logger.LogDebug("Token is expiring soon (cached). Expires: {Expiration}, Threshold: {Threshold}", 
                            cachedExpiration.Value, expirationThreshold);
                    }
                    
                    return isExpiringSoon;
                }

                // Fallback to manual decoding
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    return true;
                }

                var payload = tokenParts[1];
                var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
                var payloadJson = Encoding.UTF8.GetString(decodedPayload);
                
                var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson, _jsonOptions);
                
                if (payloadData.TryGetProperty("exp", out var expElement))
                {
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
                    var currentTime = DateTimeOffset.UtcNow;
                    var isExpiringSoon = expirationTime <= currentTime.Add(expirationThreshold);
                    
                    if (isExpiringSoon)
                    {
                        _logger.LogDebug("Token is expiring soon (manual decode). Expires: {Expiration}, Threshold: {Threshold}", 
                            expirationTime, expirationThreshold);
                    }
                    
                    return isExpiringSoon;
                }

                return true; // Consider unknown expiration as expiring soon
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if token is expiring soon");
                return true;
            }
        }

        /// <summary>
        /// Get current JWT token
        /// </summary>
        public string? GetCurrentToken()
        {
            return _userSessionService.GetJwtToken();
        }

        /// <summary>
        /// Validate token by making a test request
        /// </summary>
        public async Task<bool> ValidateTokenAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var policy = _resiliencePolicyService.CreateReadOnlyPolicy();
                
                var response = await policy.ExecuteAsync(async (context) =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "api/Auth/validate");
                    var token = GetCurrentToken();
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }
                    
                    return await httpClient.SendAsync(request);
                }, new Polly.Context("token-validation"));
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        /// <summary>
        /// Invalidate current token
        /// </summary>
        public void InvalidateToken()
        {
            _logger.LogInformation("Invalidating current token");
            _userSessionService.ClearUserSession();
        }

        /// <summary>
        /// Get token expiration time
        /// </summary>
        public async Task<DateTimeOffset?> GetTokenExpirationAsync()
        {
            try
            {
                var token = GetCurrentToken();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                // Use cached expiration first
                var cachedExpiration = _userSessionService.GetCachedTokenExpiration(token);
                if (cachedExpiration.HasValue)
                {
                    return cachedExpiration.Value;
                }

                // Decode token to get expiration
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    return null;
                }

                var payload = tokenParts[1];
                var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
                var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
                var payloadJson = Encoding.UTF8.GetString(decodedPayload);
                
                var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson, _jsonOptions);
                
                if (payloadData.TryGetProperty("exp", out var expElement))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token expiration");
                return null;
            }
        }
    }
} 