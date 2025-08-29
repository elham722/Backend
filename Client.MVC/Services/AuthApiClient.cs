using Backend.Application.Features.UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Client.MVC.Services
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly IAuthenticatedHttpClient _httpClient;
        private readonly ILogger<AuthApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthApiClient(IAuthenticatedHttpClient httpClient, ILogger<AuthApiClient> logger)
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
                _logger.LogInformation("Attempting to register user: {Email}", dto.Email);
                
                var result = await _httpClient.PostAsync<RegisterDto, AuthResultDto>("api/Auth/register", dto);
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("User registration successful: {Email}", dto.Email);
                }
                else
                {
                    _logger.LogWarning("User registration failed: {Email}. Error: {Error}", 
                        dto.Email, result?.ErrorMessage);
                }
                
                return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "Registration failed" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration: {Email}", dto.Email);
                return new AuthResultDto 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "An error occurred during registration" 
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
                _logger.LogInformation("Attempting to login user: {Email}", dto.EmailOrUsername);
                
                var result = await _httpClient.PostAsync<LoginDto, AuthResultDto>("api/Auth/login", dto);
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("User login successful: {Email}", dto.EmailOrUsername);
                }
                else
                {
                    _logger.LogWarning("User login failed: {Email}. Error: {Error}", 
                        dto.EmailOrUsername, result?.ErrorMessage);
                }
                
                return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "Login failed" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login: {Email}", dto.EmailOrUsername);
                return new AuthResultDto 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "An error occurred during login" 
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
                
                var result = await _httpClient.PostAsync<LogoutDto, AuthResultDto>("api/Auth/logout", logoutDto ?? new LogoutDto());
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("User logout successful");
                    return true;
                }
                else
                {
                    _logger.LogWarning("User logout failed. Error: {Error}", result?.ErrorMessage);
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
                var result = await _httpClient.PostAsync<RefreshTokenDto, AuthResultDto>("api/Auth/refresh-token", request);
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("Token refresh successful");
                }
                else
                {
                    _logger.LogWarning("Token refresh failed. Error: {Error}", result?.ErrorMessage);
                }
                
                return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "Token refresh failed" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new AuthResultDto 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "An error occurred during token refresh" 
                };
            }
        }

        // Note: ValidateTokenAsync method removed as it's not needed for regular requests
        // JWT tokens are self-contained and validated by AuthenticationInterceptor
        // This method was primarily useful for background jobs or external validations
    }
} 