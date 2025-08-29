using Backend.Application.Features.UserManagement.DTOs;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Client.MVC.Services
{
    /// <summary>
    /// Implementation of authentication interceptor for automatic JWT token management
    /// </summary>
    public class AuthenticationInterceptor : IAuthenticationInterceptor
    {
        private readonly IUserSessionService _userSessionService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthenticationInterceptor> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

            public AuthenticationInterceptor(
        IUserSessionService userSessionService,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthenticationInterceptor> logger)
        {
            _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Add authentication header to the request
        /// </summary>
        public async Task<HttpRequestMessage> AddAuthenticationHeaderAsync(HttpRequestMessage request)
        {
            try
            {
                // Check if token is valid and refresh if needed
                if (!IsTokenValid())
                {
                    var refreshSuccess = await RefreshTokenIfNeededAsync();
                    if (!refreshSuccess)
                    {
                        _logger.LogWarning("Failed to refresh token, proceeding without authentication");
                        return request;
                    }
                }

                var token = GetCurrentToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogDebug("Authentication header added to request: {Method} {Url}", 
                        request.Method, request.RequestUri);
                }
                else
                {
                    _logger.LogDebug("No valid token available for request: {Method} {Url}", 
                        request.Method, request.RequestUri);
                }

                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding authentication header to request");
                return request;
            }
        }

        /// <summary>
        /// Handle authentication errors (401, 403)
        /// </summary>
        public async Task<bool> HandleAuthenticationErrorAsync(HttpResponseMessage response)
        {
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Received 401 Unauthorized, attempting token refresh");
                    
                    var refreshSuccess = await RefreshTokenIfNeededAsync();
                    if (refreshSuccess)
                    {
                        _logger.LogInformation("Token refreshed successfully after 401 error");
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to refresh token after 401 error");
                        // Clear invalid session
                        _userSessionService.ClearUserSession();
                        return false;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("Received 403 Forbidden - insufficient permissions");
                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling authentication error");
                return false;
            }
        }

        /// <summary>
        /// Refresh token if needed
        /// </summary>
        public async Task<bool> RefreshTokenIfNeededAsync()
        {
            try
            {
                var refreshToken = _userSessionService.GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("No refresh token available");
                    return false;
                }

                _logger.LogInformation("Attempting to refresh access token");
                
                // Create refresh token request
                var request = new RefreshTokenDto { RefreshToken = refreshToken };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Make direct HTTP request to refresh token endpoint
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.PostAsync("api/Auth/refresh-token", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuthResultDto>(responseContent, _jsonOptions);
                    
                    if (result?.IsSuccess == true && !string.IsNullOrEmpty(result.AccessToken))
                    {
                        // Update both JWT token and refresh token in session and cookies
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
                
                var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson, _jsonOptions);
                
                // Check expiration
                if (payloadData.TryGetProperty("exp", out var expElement))
                {
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
                    var currentTime = DateTimeOffset.UtcNow;
                    
                    // Token is valid if it expires in more than 5 minutes
                    var isValid = expirationTime > currentTime.AddMinutes(5);
                    
                    if (!isValid)
                    {
                        _logger.LogDebug("Token is expired or expiring soon. Expires: {Expiration}, Current: {Current}", 
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
        /// Get current JWT token
        /// </summary>
        public string? GetCurrentToken()
        {
            return _userSessionService.GetJwtToken();
        }
    }
} 