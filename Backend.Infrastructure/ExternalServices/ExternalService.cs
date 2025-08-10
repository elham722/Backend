using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Backend.Infrastructure.LocalStorage;

namespace Backend.Infrastructure.ExternalServices
{
    public class ExternalService : IExternalService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalServiceOptions _options;
        private readonly ILocalStorageService _localStorage;

        public ExternalService(HttpClient httpClient, IOptions<ExternalServiceOptions> options, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _localStorage = localStorage;
            
            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
            
            // Add default headers (excluding restricted headers)
            var restrictedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Content-Type",
                "Content-Length",
                "Host",
                "Connection",
                "Transfer-Encoding"
            };
            
            foreach (var header in _options.DefaultHeaders)
            {
                if (!restrictedHeaders.Contains(header.Key))
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            
            // Add Bearer token if exists
            AddBearerToken();
        }

        private void AddBearerToken()
        {
            if (_localStorage.Exists("token"))
            {
                var token = _localStorage.GetStorageValue<string>("token");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<TResponse> GetAsync<TResponse>(string endpoint, object query = null)
        {
            var url = BuildUrl(endpoint, query);
            var response = await _httpClient.GetAsync(url);
            
            await EnsureSuccessStatusCode(response);
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            
            await EnsureSuccessStatusCode(response);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<TResponse> PostAsync<TResponse>(string endpoint)
        {
            var response = await _httpClient.PostAsync(endpoint, null);
            
            await EnsureSuccessStatusCode(response);
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            
            await EnsureSuccessStatusCode(response);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            await EnsureSuccessStatusCode(response);
        }

        private string BuildUrl(string endpoint, object query)
        {
            if (query == null) return endpoint;
            
            var queryString = string.Join("&", 
                query.GetType().GetProperties()
                    .Where(p => p.GetValue(query) != null)
                    .Select(p => $"{p.Name}={Uri.EscapeDataString(p.GetValue(query).ToString())}"));
            
            return string.IsNullOrEmpty(queryString) ? endpoint : $"{endpoint}?{queryString}";
        }

        private async Task EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {response.StatusCode}. {errorContent}");
            }
        }
    }
} 