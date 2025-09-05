using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Client.MVC.Services.Abstractions;

namespace Client.MVC.Services.Implementations
{
    /// <summary>
    /// Implementation of ILogoutService that handles logout operations
    /// Coordinates between API calls and session management without circular dependencies
    /// </summary>
    public class LogoutService : ILogoutService
    {
        private readonly ISessionManager _sessionManager;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<LogoutService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LogoutService(
            ISessionManager sessionManager,
            ITokenProvider tokenProvider,
            ILogger<LogoutService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Logout user from API and clear local session
        /// </summary>
        public async Task<ApiResponse<LogoutResultDto>> LogoutAsync(bool logoutFromAllDevices = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get logout data before clearing session
                var logoutDto = _sessionManager.GetLogoutDto();
                logoutDto.LogoutFromAllDevices = logoutFromAllDevices;

                // Call API logout directly to avoid circular dependency
                var apiResult = await CallLogoutApiAsync(logoutDto, cancellationToken);
                
                // Clear local session regardless of API response
                await _sessionManager.ClearUserSessionAsync(cancellationToken);

                return apiResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to logout user");
                
                // Still clear local session on error
                await _sessionManager.ClearUserSessionAsync(cancellationToken);
                
                return ApiResponse<LogoutResultDto>.Failure("خطا در خروج از سیستم");
            }
        }

        /// <summary>
        /// Logout only from local session (no API call)
        /// </summary>
        public async Task<ApiResponse<LogoutResultDto>> LogoutLocalAsync()
        {
            try
            {
                await _sessionManager.ClearUserSessionAsync();
                
                return ApiResponse<LogoutResultDto>.Success(new LogoutResultDto
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    LogoutTime = DateTime.UtcNow,
                    LogoutFromAllDevices = false,
                    TokensInvalidated = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear local session");
                return ApiResponse<LogoutResultDto>.Failure("خطا در پاک کردن session محلی");
            }
        }

        /// <summary>
        /// Call logout API directly without using IAuthApiClient to avoid circular dependency
        /// </summary>
        private async Task<ApiResponse<LogoutResultDto>> CallLogoutApiAsync(LogoutDto logoutDto, CancellationToken cancellationToken)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                
                // Get current JWT token for authorization
                var jwtToken = _tokenProvider.GetJwtToken();
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
                }

                var jsonContent = System.Text.Json.JsonSerializer.Serialize(logoutDto);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/auth/logout", content, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<LogoutResultDto>>(responseContent);
                    
                    return apiResponse ?? ApiResponse<LogoutResultDto>.Success(new LogoutResultDto
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        LogoutTime = DateTime.UtcNow,
                        LogoutFromAllDevices = logoutDto.LogoutFromAllDevices,
                        TokensInvalidated = 1
                    });
                }
                else
                {
                    _logger.LogWarning("Logout API call failed with status: {StatusCode}", response.StatusCode);
                    return ApiResponse<LogoutResultDto>.Failure($"Logout failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling logout API");
                return ApiResponse<LogoutResultDto>.Failure("خطا در فراخوانی API خروج");
            }
        }
    }
}