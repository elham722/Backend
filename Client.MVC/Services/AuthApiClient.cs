using Backend.Application.Features.UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Client.MVC.Services
{
    /// <summary>
    /// Typed API client implementation for authentication operations using HttpClient
    /// </summary>
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with email: {Email}", dto.Email);
                
                var json = JsonSerializer.Serialize(dto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/Auth/register", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuthResultDto>(responseContent, _jsonOptions);
                    
                    if (result?.IsSuccess == true)
                    {
                        _logger.LogInformation("User registration successful for email: {Email}", dto.Email);
                    }
                    else
                    {
                        _logger.LogWarning("User registration failed for email: {Email}. Error: {Error}", 
                            dto.Email, result?.ErrorMessage);
                    }
                    
                    return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "Invalid response format" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("User registration failed with status {StatusCode}. Error: {Error}", 
                        response.StatusCode, errorContent);
                    
                    return new AuthResultDto 
                    { 
                        IsSuccess = false, 
                        ErrorMessage = $"Server error: {response.StatusCode}" 
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", dto.Email);
                return new AuthResultDto 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "خطا در ارتباط با سرور" 
                };
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        public async Task<AuthResultDto> LoginAsync(LoginDto dto)
        {
            try
            {
                _logger.LogInformation("Attempting to login user with email: {Email}", dto.EmailOrUsername);
                
                var json = JsonSerializer.Serialize(dto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/Auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuthResultDto>(responseContent, _jsonOptions);
                    
                    if (result?.IsSuccess == true)
                    {
                        _logger.LogInformation("User login successful for email: {Email}", dto.EmailOrUsername);
                    }
                    else
                    {
                        _logger.LogWarning("User login failed for email: {Email}. Error: {Error}", 
                            dto.EmailOrUsername, result?.ErrorMessage);
                    }
                    
                    return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "Invalid response format" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("User login failed with status {StatusCode}. Error: {Error}", 
                        response.StatusCode, errorContent);
                    
                    return new AuthResultDto 
                    { 
                        IsSuccess = false, 
                        ErrorMessage = $"Server error: {response.StatusCode}" 
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login for email: {Email}", dto.EmailOrUsername);
                return new AuthResultDto 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "خطا در ارتباط با سرور" 
                };
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        public async Task<bool> LogoutAsync(LogoutDto? logoutDto = null)
        {
            try
            {
                _logger.LogInformation("Attempting to logout user");
                
                var logoutRequest = logoutDto ?? new LogoutDto();
                var json = JsonSerializer.Serialize(logoutRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/Auth/logout", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("User logout successful");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("User logout failed with status {StatusCode}. Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user logout");
                return false;
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Attempting to refresh token");
                
                var request = new RefreshTokenDto { RefreshToken = refreshToken };
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("api/Auth/refresh-token", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuthResultDto>(responseContent, _jsonOptions);
                    
                    if (result?.IsSuccess == true)
                    {
                        _logger.LogInformation("Token refresh successful");
                    }
                    else
                    {
                        _logger.LogWarning("Token refresh failed. Error: {Error}", result?.ErrorMessage);
                    }
                    
                    return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "Invalid response format" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Token refresh failed with status {StatusCode}. Error: {Error}", 
                        response.StatusCode, errorContent);
                    
                    return new AuthResultDto 
                    { 
                        IsSuccess = false, 
                        ErrorMessage = $"Server error: {response.StatusCode}" 
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new AuthResultDto 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "خطا در تمدید توکن" 
                };
            }
        }

        /// <summary>
        /// Validate current token
        /// </summary>
        public async Task<bool> ValidateTokenAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to validate token");
                
                var response = await _httpClient.GetAsync("api/Auth/validate-token");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ValidateTokenDto>(responseContent, _jsonOptions);
                    
                    if (result?.IsValid == true)
                    {
                        _logger.LogInformation("Token validation successful");
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Token validation failed. Error: {Error}", result?.ErrorMessage);
                        return false;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Token validation failed with status {StatusCode}. Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }
    }
} 