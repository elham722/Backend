using Backend.Application.Features.UserManagement.DTOs;
using Backend.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;

namespace Client.MVC.Services
{
    /// <summary>
    /// Typed API client implementation for authentication operations
    /// </summary>
    public class AuthApiClient : IAuthApiClient
    {
        private readonly IExternalService _externalService;
        private readonly ILogger<AuthApiClient> _logger;

        public AuthApiClient(IExternalService externalService, ILogger<AuthApiClient> logger)
        {
            _externalService = externalService ?? throw new ArgumentNullException(nameof(externalService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with email: {Email}", dto.Email);
                
                var result = await _externalService.PostAsync<RegisterDto, AuthResultDto>("api/Auth/register", dto);
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("User registration successful for email: {Email}", dto.Email);
                }
                else
                {
                    _logger.LogWarning("User registration failed for email: {Email}. Error: {Error}", 
                        dto.Email, result?.ErrorMessage);
                }
                
                return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "No response from server" };
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
                
                var result = await _externalService.PostAsync<LoginDto, AuthResultDto>("api/Auth/login", dto);
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("User login successful for email: {Email}", dto.EmailOrUsername);
                }
                else
                {
                    _logger.LogWarning("User login failed for email: {Email}. Error: {Error}", 
                        dto.EmailOrUsername, result?.ErrorMessage);
                }
                
                return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "No response from server" };
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
                await _externalService.PostAsync<LogoutDto, object>("api/Auth/logout", logoutRequest);
                
                _logger.LogInformation("User logout successful");
                return true;
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
                var result = await _externalService.PostAsync<RefreshTokenDto, AuthResultDto>("api/Auth/refresh-token", request);
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("Token refresh successful");
                }
                else
                {
                    _logger.LogWarning("Token refresh failed. Error: {Error}", result?.ErrorMessage);
                }
                
                return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "No response from server" };
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
                
                var result = await _externalService.GetAsync<ValidateTokenDto>("api/Auth/validate-token");
                
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }
    }
} 