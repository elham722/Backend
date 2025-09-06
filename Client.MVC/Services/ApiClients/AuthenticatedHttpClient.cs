using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Polly;
using Backend.Application.Common.Results;
using Client.MVC.Services.Abstractions;
using Client.MVC.Services.Infrastructure;

namespace Client.MVC.Services.ApiClients
{
    /// <summary>
    /// Implementation of authenticated HTTP client with automatic token management and resilience policies
    /// </summary>
    public class AuthenticatedHttpClient : IAuthenticatedHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticationInterceptor _authInterceptor;
        private readonly ILogger<AuthenticatedHttpClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Dictionary<string, string> _customHeaders;
        private readonly ResiliencePolicyService _resiliencePolicyService;
        private readonly IConcurrencyManager _concurrencyManager;

        public AuthenticatedHttpClient(
            IHttpClientFactory httpClientFactory,
            IAuthenticationInterceptor authInterceptor,
            ILogger<AuthenticatedHttpClient> logger,
            ResiliencePolicyService resiliencePolicyService,
            IConcurrencyManager concurrencyManager)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _authInterceptor = authInterceptor ?? throw new ArgumentNullException(nameof(authInterceptor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _resiliencePolicyService = resiliencePolicyService ?? throw new ArgumentNullException(nameof(resiliencePolicyService));
            _concurrencyManager = concurrencyManager ?? throw new ArgumentNullException(nameof(concurrencyManager));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            
            _customHeaders = new Dictionary<string, string>();

            // Subscribe to logout required event
            _authInterceptor.OnLogoutRequired += OnLogoutRequired;
        }

        /// <summary>
        /// Handle logout required event from authentication interceptor
        /// </summary>
        private void OnLogoutRequired(object? sender, EventArgs e)
        {
            _logger.LogWarning("Logout required due to refresh token failure");
            
            // Clear any custom headers that might be related to authentication
            _customHeaders.Clear();
            
            // Additional cleanup can be added here
            // For example, notify other components about the logout
        }

        /// <summary>
        /// Send GET request with automatic authentication and resilience policies
        /// </summary>
        public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default) where TResponse : class
        {
            try
            {
                _logger.LogDebug("Sending GET request to: {Endpoint}", endpoint);
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var policy = _resiliencePolicyService.CreateReadOnlyPolicy();
                
                var response = await policy.ExecuteAsync(async (context) =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                    
                    // Add authentication header
                    request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
                    
                    // Add custom headers
                    AddCustomHeadersToRequest(request);
                    
                    var httpResponse = await httpClient.SendAsync(request, cancellationToken);
                    
                    // Handle authentication errors
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                        httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        var handled = await _authInterceptor.HandleAuthenticationErrorAsync(httpResponse);
                        if (handled)
                        {
                            // Wait for refresh completion before retrying
                            var refreshCompleted = await _authInterceptor.WaitForRefreshCompletionAsync();
                            if (refreshCompleted)
                            {
                                // Retry the request with new token
                                request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                                request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
                                AddCustomHeadersToRequest(request);
                                httpResponse = await httpClient.SendAsync(request, cancellationToken);
                            }
                        }
                    }
                    
                    return httpResponse;
                }, new Context(endpoint));
                
                return await HandleResponseAsync<TResponse>(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("GET request to {Endpoint} was cancelled", endpoint);
                return ApiResponse<TResponse>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending GET request to: {Endpoint}", endpoint);
                return ApiResponse<TResponse>.Error($"Error sending GET request: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Send POST request with automatic authentication and resilience policies
        /// </summary>
        public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default) 
            where TRequest : class 
            where TResponse : class
        {
            try
            {
                _logger.LogDebug("Sending POST request to: {Endpoint}", endpoint);
                
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                
                // Use auth policy for sensitive operations like login/register
                var isAuthOperation = endpoint.Contains("/auth/", StringComparison.OrdinalIgnoreCase) ||
                                    endpoint.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
                                    endpoint.Contains("/register", StringComparison.OrdinalIgnoreCase) ||
                                    endpoint.Contains("/refresh", StringComparison.OrdinalIgnoreCase);
                
                var policy = isAuthOperation 
                    ? _resiliencePolicyService.CreateAuthPolicy() 
                    : _resiliencePolicyService.CreateGeneralPolicy();
                
                var response = await policy.ExecuteAsync(async (context) =>
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = content
                    };
                    
                    // Add authentication header
                    httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                    
                    // Add custom headers
                    AddCustomHeadersToRequest(httpRequest);
                    
                    var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
                    
                    // Handle authentication errors
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                        httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        var handled = await _authInterceptor.HandleAuthenticationErrorAsync(httpResponse);
                        if (handled)
                        {
                            // Wait for refresh completion before retrying
                            var refreshCompleted = await _authInterceptor.WaitForRefreshCompletionAsync();
                            if (refreshCompleted)
                            {
                                // Retry the request with new token
                                httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                                {
                                    Content = content
                                };
                                httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                                AddCustomHeadersToRequest(httpRequest);
                                httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
                            }
                        }
                    }
                    
                    return httpResponse;
                }, new Context(endpoint));
                
                return await HandleResponseAsync<TResponse>(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("POST request to {Endpoint} was cancelled", endpoint);
                return ApiResponse<TResponse>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending POST request to: {Endpoint}", endpoint);
                return ApiResponse<TResponse>.Error($"Error sending POST request: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Send PUT request with automatic authentication and resilience policies
        /// </summary>
        public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default) 
            where TRequest : class 
            where TResponse : class
        {
            try
            {
                _logger.LogDebug("Sending PUT request to: {Endpoint}", endpoint);
                
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var policy = _resiliencePolicyService.CreateGeneralPolicy();
                
                var response = await policy.ExecuteAsync(async (context) =>
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
                    {
                        Content = content
                    };
                    
                    // Add authentication header
                    httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                    
                    // Add custom headers
                    AddCustomHeadersToRequest(httpRequest);
                    
                    var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
                    
                    // Handle authentication errors
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                        httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        var handled = await _authInterceptor.HandleAuthenticationErrorAsync(httpResponse);
                        if (handled)
                        {
                            // Wait for refresh completion before retrying
                            var refreshCompleted = await _authInterceptor.WaitForRefreshCompletionAsync();
                            if (refreshCompleted)
                            {
                                // Retry the request with new token
                                httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
                                {
                                    Content = content
                                };
                                httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                                AddCustomHeadersToRequest(httpRequest);
                                httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
                            }
                        }
                    }
                    
                    return httpResponse;
                }, new Context(endpoint));
                
                return await HandleResponseAsync<TResponse>(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("PUT request to {Endpoint} was cancelled", endpoint);
                return ApiResponse<TResponse>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending PUT request to: {Endpoint}", endpoint);
                return ApiResponse<TResponse>.Error($"Error sending PUT request: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Send DELETE request with automatic authentication and resilience policies
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Sending DELETE request to: {Endpoint}", endpoint);
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var policy = _resiliencePolicyService.CreateGeneralPolicy();
                
                var response = await policy.ExecuteAsync(async (context) =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                    
                    // Add authentication header
                    request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
                    
                    // Add custom headers
                    AddCustomHeadersToRequest(request);
                    
                    var httpResponse = await httpClient.SendAsync(request, cancellationToken);
                    
                    // Handle authentication errors
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                        httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        var handled = await _authInterceptor.HandleAuthenticationErrorAsync(httpResponse);
                        if (handled)
                        {
                            // Wait for refresh completion before retrying
                            var refreshCompleted = await _authInterceptor.WaitForRefreshCompletionAsync();
                            if (refreshCompleted)
                            {
                                // Retry the request with new token
                                request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                                request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
                                AddCustomHeadersToRequest(request);
                                httpResponse = await httpClient.SendAsync(request, cancellationToken);
                            }
                        }
                    }
                    
                    return httpResponse;
                }, new Context(endpoint));
                
                if (response.IsSuccessStatusCode)
                {
                    return ApiResponse<bool>.Success(true, (int)response.StatusCode);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("DELETE request failed with status {StatusCode}: {Error}", 
                        response.StatusCode, errorContent);
                    return ApiResponse<bool>.Error($"DELETE request failed: {response.StatusCode}", (int)response.StatusCode);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DELETE request to {Endpoint} was cancelled", endpoint);
                return ApiResponse<bool>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending DELETE request to: {Endpoint}", endpoint);
                return ApiResponse<bool>.Error($"Error sending DELETE request: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Send PATCH request with automatic authentication and resilience policies
        /// </summary>
        public async Task<ApiResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default) 
            where TRequest : class 
            where TResponse : class
        {
            try
            {
                _logger.LogDebug("Sending PATCH request to: {Endpoint}", endpoint);
                
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var policy = _resiliencePolicyService.CreateGeneralPolicy();
                
                var response = await policy.ExecuteAsync(async (context) =>
                {
                    var httpRequest = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                    {
                        Content = content
                    };
                    
                    // Add authentication header
                    httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                    
                    // Add custom headers
                    AddCustomHeadersToRequest(httpRequest);
                    
                    var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
                    
                    // Handle authentication errors
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                        httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        var handled = await _authInterceptor.HandleAuthenticationErrorAsync(httpResponse);
                        if (handled)
                        {
                            // Wait for refresh completion before retrying
                            var refreshCompleted = await _authInterceptor.WaitForRefreshCompletionAsync();
                            if (refreshCompleted)
                            {
                                // Retry the request with new token
                                httpRequest = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                                {
                                    Content = content
                                };
                                httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                                AddCustomHeadersToRequest(httpRequest);
                                httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
                            }
                        }
                    }
                    
                    return httpResponse;
                }, new Context(endpoint));
                
                return await HandleResponseAsync<TResponse>(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("PATCH request to {Endpoint} was cancelled", endpoint);
                return ApiResponse<TResponse>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending PATCH request to: {Endpoint}", endpoint);
                return ApiResponse<TResponse>.Error($"Error sending PATCH request: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Add custom header to all requests
        /// </summary>
        public void AddHeader(string name, string value)
        {
            _customHeaders[name] = value;
            _logger.LogDebug("Added custom header: {Name} = {Value}", name, value);
        }

        /// <summary>
        /// Remove custom header
        /// </summary>
        public void RemoveHeader(string name)
        {
            if (_customHeaders.Remove(name))
            {
                _logger.LogDebug("Removed custom header: {Name}", name);
            }
        }

        /// <summary>
        /// Clear all custom headers
        /// </summary>
        public void ClearHeaders()
        {
            _customHeaders.Clear();
            _logger.LogDebug("Cleared all custom headers");
        }

        /// <summary>
        /// Handle HTTP response and deserialize to specified type
        /// </summary>
        private async Task<ApiResponse<TResponse>> HandleResponseAsync<TResponse>(HttpResponseMessage response) where TResponse : class
        {
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    if (typeof(TResponse) == typeof(string))
                    {
                        return ApiResponse<TResponse>.Success(content as TResponse, (int)response.StatusCode);
                    }
                    
                    if (string.IsNullOrEmpty(content))
                    {
                        // Return success with null data for empty responses
                        return ApiResponse<TResponse>.Success(null!, (int)response.StatusCode);
                    }
                    
                    var result = JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);
                    _logger.LogDebug("Successfully deserialized response to {Type}", typeof(TResponse).Name);
                    return ApiResponse<TResponse>.Success(result!, (int)response.StatusCode);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Request failed with status {StatusCode}: {Error}", 
                        response.StatusCode, errorContent);
                    return ApiResponse<TResponse>.Error($"Request failed: {response.StatusCode}", (int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling response");
                return ApiResponse<TResponse>.Error($"Error handling response: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Add custom headers to HTTP request
        /// </summary>
        private void AddCustomHeadersToRequest(HttpRequestMessage request)
        {
            foreach (var header in _customHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            
            // Add User-Agent header if not already present
            if (!request.Headers.Contains("User-Agent"))
            {
                request.Headers.Add("User-Agent", "Backend-Client/1.0");
            }
        }
    }
} 