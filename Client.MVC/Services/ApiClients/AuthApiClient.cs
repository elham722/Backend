using Backend.Application.Features.UserManagement.DTOs;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Client.MVC.Services.Abstractions;
using Client.MVC.Services.Infrastructure;

namespace Client.MVC.Services.ApiClients
{
    public static class AuthResultExtensions
    {
        public static ApiResponse<LoginResponse> ToApiResponse(this LoginResponse? result, int defaultStatusCode = 400)
        {
            if (result?.IsSuccess == true)
            {
                return ApiResponse<LoginResponse>.Success(result, 200);
            }
            else
            {
                return ApiResponse<LoginResponse>.Error(result?.ErrorMessage ?? "Operation failed", defaultStatusCode);
            }
        }
    }

    public class AuthApiClient : IAuthApiClient
    {
        private readonly IAuthenticatedHttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthApiClient(IAuthenticatedHttpClient httpClient, IHttpClientFactory httpClientFactory, ILogger<AuthApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // مهم، اگر API PascalCase دارد
                PropertyNameCaseInsensitive = true, // تا مطمئن شود حتی اگر کمی تفاوت باشد map شود
                WriteIndented = false
            };
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogUserAuthentication("Register", dto.Email, true);
                
                var response = await _httpClient.PostAsync<RegisterDto, LoginResponse>("api/v1.0/auth/register", dto, cancellationToken);
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogInformation("Registration successful - AccessToken: {AccessToken}, RefreshToken: {RefreshToken}", 
                        response.Data.AccessToken, response.Data.RefreshToken);
                    return ApiResponse<LoginResponse>.Success(response.Data);
                }
                else
                {
                    var errorMessage = response.Data?.ErrorMessage ?? response.ErrorMessage ?? "Registration failed";
                    _logger.LogWarning("Registration failed: {ErrorMessage}", errorMessage);
                    return ApiResponse<LoginResponse>.Error(errorMessage, response.StatusCode ?? 400);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogUserAuthentication("Register", dto.Email, false, "Operation was cancelled");
                return ApiResponse<LoginResponse>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogErrorSecurely(ex, "Register", null, $"Email={dto.Email}");
                return ApiResponse<LoginResponse>.Error("An error occurred during registration", 500);
            }
        }

        /// <summary>
        /// Login user - GUARANTEED DEBUG VERSION
        /// </summary>
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest dto, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("=== LOGIN DEBUG START ===");
                _logger.LogInformation("Login attempt for: {EmailOrUsername}", dto.EmailOrUsername);
                
                // Step 1: Direct HTTP call to see raw response
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var json = JsonSerializer.Serialize(dto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Request JSON: {RequestJson}", json);
                
                var directResponse = await httpClient.PostAsync("api/v1.0/auth/login", content, cancellationToken);
                var rawContent = await directResponse.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Direct HTTP Status: {StatusCode}", directResponse.StatusCode);
                _logger.LogInformation("Direct Raw Response: {RawContent}", rawContent);
                
                // Step 2: Test deserialization with different options
                LoginResponse? testResult = null;
                try
                {
                    // Test with current options
                    testResult = JsonSerializer.Deserialize<LoginResponse>(rawContent, _jsonOptions);
                    _logger.LogInformation("Test deserialization - AccessToken: {AccessToken}, RefreshToken: {RefreshToken}", 
                        testResult?.AccessToken, testResult?.RefreshToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Test deserialization failed");
                }
                
                // Step 3: Use the normal flow
                var response = await _httpClient.PostAsync<LoginRequest, LoginResponse>("api/v1.0/auth/login", dto, cancellationToken);
                
                _logger.LogInformation("Normal flow response - IsSuccess: {IsSuccess}, Data: {Data}", 
                    response.IsSuccess, response.Data != null ? "Not null" : "NULL");
                
                if (response.Data != null)
                {
                    _logger.LogInformation("Normal flow - AccessToken: {AccessToken}, RefreshToken: {RefreshToken}", 
                        response.Data.AccessToken, response.Data.RefreshToken);
                }
                
                _logger.LogInformation("=== LOGIN DEBUG END ===");
                
                if (response.IsSuccess && response.Data != null)
                {
                    return ApiResponse<LoginResponse>.Success(response.Data);
                }
                else
                {
                    var errorMessage = response.Data?.ErrorMessage ?? response.ErrorMessage ?? "Login failed";
                    _logger.LogWarning("Login failed: {ErrorMessage}", errorMessage);
                    return ApiResponse<LoginResponse>.Error(errorMessage, response.StatusCode ?? 400);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogUserAuthentication("Login", dto.EmailOrUsername, false, "Operation was cancelled");
                return ApiResponse<LoginResponse>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogErrorSecurely(ex, "Login", null, $"Email={dto.EmailOrUsername}");
                return ApiResponse<LoginResponse>.Error("An error occurred during login", 500);
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        public async Task<ApiResponse<LogoutResultDto>> LogoutAsync(LogoutDto? logoutDto = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to logout user");
                
                var response = await _httpClient.PostAsync<LogoutDto, LoginResponse>("api/v1.0/auth/logout", logoutDto ?? new LogoutDto(), cancellationToken);
                
                if (response.IsSuccess && response.Data?.IsSuccess == true)
                {
                    _logger.LogInformation("User logout successful");
                    var logoutResult = new LogoutResultDto
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        LogoutTime = DateTime.UtcNow
                    };
                    return ApiResponse<LogoutResultDto>.Success(logoutResult);
                }
                else
                {
                    var errorMessage = response.Data?.ErrorMessage ?? response.ErrorMessage ?? "Logout failed";
                    _logger.LogWarning("User logout failed. Error: {Error}", errorMessage);
                    var logoutResult = new LogoutResultDto
                    {
                        IsSuccess = false,
                        ErrorMessage = errorMessage,
                        StatusCode = response.StatusCode ?? 400,
                        LogoutTime = DateTime.UtcNow
                    };
                    return ApiResponse<LogoutResultDto>.Error(errorMessage, response.StatusCode ?? 400);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("User logout cancelled");
                return ApiResponse<LogoutResultDto>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user logout");
                return ApiResponse<LogoutResultDto>.Error("An error occurred during logout", 500);
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to refresh token");
                
                var request = new RefreshTokenDto { RefreshToken = refreshToken };
                var response = await _httpClient.PostAsync<RefreshTokenDto, LoginResponse>("api/v1.0/auth/refresh-token", request, cancellationToken);
                
                if (response.IsSuccess && response.Data?.IsSuccess == true)
                {
                    _logger.LogInformation("Token refresh successful");
                    return ApiResponse<LoginResponse>.Success(response.Data);
                }
                else
                {
                    var errorMessage = response.Data?.ErrorMessage ?? response.ErrorMessage ?? "Token refresh failed";
                    _logger.LogWarning("Token refresh failed. Error: {Error}", errorMessage);
                    return ApiResponse<LoginResponse>.Error(errorMessage, response.StatusCode ?? 400);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Token refresh cancelled");
                return ApiResponse<LoginResponse>.Cancelled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ApiResponse<LoginResponse>.Error("An error occurred during token refresh", 500);
            }
        }

        // Note: ValidateTokenAsync method removed as it's not needed for regular requests
        // JWT tokens are self-contained and validated by AuthenticationInterceptor
        // This method was primarily useful for background jobs or external validations
    }
} 