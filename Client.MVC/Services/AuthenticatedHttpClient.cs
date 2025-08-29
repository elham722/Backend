using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Client.MVC.Services
{
    /// <summary>
    /// Implementation of authenticated HTTP client with automatic token management
    /// </summary>
    public class AuthenticatedHttpClient : IAuthenticatedHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthenticationInterceptor _authInterceptor;
        private readonly ILogger<AuthenticatedHttpClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Dictionary<string, string> _customHeaders;

            public AuthenticatedHttpClient(
        IHttpClientFactory httpClientFactory,
        IAuthenticationInterceptor authInterceptor,
        ILogger<AuthenticatedHttpClient> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _authInterceptor = authInterceptor ?? throw new ArgumentNullException(nameof(authInterceptor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            
            _customHeaders = new Dictionary<string, string>();
        }

        /// <summary>
        /// Send GET request with automatic authentication
        /// </summary>
        public async Task<TResponse?> GetAsync<TResponse>(string endpoint) where TResponse : class
        {
            try
            {
                _logger.LogDebug("Sending GET request to: {Endpoint}", endpoint);
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                
                // Add authentication header
                request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
                
                // Add custom headers
                AddCustomHeadersToRequest(request);
                
                var response = await httpClient.SendAsync(request);
                
                // Handle authentication errors
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var handled = await _authInterceptor.HandleAuthenticationErrorAsync(response);
                    if (handled)
                    {
                        // Retry the request with new token
                        request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                        request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
                        AddCustomHeadersToRequest(request);
                        response = await httpClient.SendAsync(request);
                    }
                }
                
                return await HandleResponseAsync<TResponse>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending GET request to: {Endpoint}", endpoint);
                return null;
            }
        }

        /// <summary>
        /// Send POST request with automatic authentication
        /// </summary>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request) 
            where TRequest : class 
            where TResponse : class
        {
            try
            {
                _logger.LogDebug("Sending POST request to: {Endpoint}", endpoint);
                
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = content
                };
                
                // Add authentication header
                httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                
                // Add custom headers
                AddCustomHeadersToRequest(httpRequest);
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.SendAsync(httpRequest);
                
                // Handle authentication errors
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var handled = await _authInterceptor.HandleAuthenticationErrorAsync(response);
                    if (handled)
                    {
                        // Retry the request with new token
                        httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                        {
                            Content = content
                        };
                        httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                        AddCustomHeadersToRequest(httpRequest);
                        response = await httpClient.SendAsync(httpRequest);
                    }
                }
                
                return await HandleResponseAsync<TResponse>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending POST request to: {Endpoint}", endpoint);
                return null;
            }
        }

        /// <summary>
        /// Send PUT request with automatic authentication
        /// </summary>
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request) 
            where TRequest : class 
            where TResponse : class
        {
            try
            {
                _logger.LogDebug("Sending PUT request to: {Endpoint}", endpoint);
                
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
                {
                    Content = content
                };
                
                // Add authentication header
                httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                
                // Add custom headers
                AddCustomHeadersToRequest(httpRequest);
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.SendAsync(httpRequest);
                
                // Handle authentication errors
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var handled = await _authInterceptor.HandleAuthenticationErrorAsync(response);
                    if (handled)
                    {
                        // Retry the request with new token
                        httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint)
                        {
                            Content = content
                        };
                        httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                        AddCustomHeadersToRequest(httpRequest);
                        response = await httpClient.SendAsync(httpRequest);
                    }
                }
                
                return await HandleResponseAsync<TResponse>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending PUT request to: {Endpoint}", endpoint);
                return null;
            }
        }

        /// <summary>
        /// Send DELETE request with automatic authentication
        /// </summary>
        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogDebug("Sending DELETE request to: {Endpoint}", endpoint);
                
                var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                
                // Add authentication header
                request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
                
                // Add custom headers
                AddCustomHeadersToRequest(request);
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.SendAsync(request);
                
                // Handle authentication errors
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var handled = await _authInterceptor.HandleAuthenticationErrorAsync(response);
                    if (handled)
                    {
                        // Retry the request with new token
                        request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                        request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
                        AddCustomHeadersToRequest(request);
                        response = await httpClient.SendAsync(request);
                    }
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending DELETE request to: {Endpoint}", endpoint);
                return false;
            }
        }

        /// <summary>
        /// Send PATCH request with automatic authentication
        /// </summary>
        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest request) 
            where TRequest : class 
            where TResponse : class
        {
            try
            {
                _logger.LogDebug("Sending PATCH request to: {Endpoint}", endpoint);
                
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                {
                    Content = content
                };
                
                // Add authentication header
                httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                
                // Add custom headers
                AddCustomHeadersToRequest(httpRequest);
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.SendAsync(httpRequest);
                
                // Handle authentication errors
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var handled = await _authInterceptor.HandleAuthenticationErrorAsync(response);
                    if (handled)
                    {
                        // Retry the request with new token
                        httpRequest = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                        {
                            Content = content
                        };
                        httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
                        AddCustomHeadersToRequest(httpRequest);
                        response = await httpClient.SendAsync(httpRequest);
                    }
                }
                
                return await HandleResponseAsync<TResponse>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending PATCH request to: {Endpoint}", endpoint);
                return null;
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
        private async Task<TResponse?> HandleResponseAsync<TResponse>(HttpResponseMessage response) where TResponse : class
        {
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    if (typeof(TResponse) == typeof(string))
                    {
                        return content as TResponse;
                    }
                    
                    if (string.IsNullOrEmpty(content))
                    {
                        return null;
                    }
                    
                    var result = JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);
                    _logger.LogDebug("Successfully deserialized response to {Type}", typeof(TResponse).Name);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Request failed with status {StatusCode}: {Error}", 
                        response.StatusCode, errorContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling response");
                return null;
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
        }
    }
} 