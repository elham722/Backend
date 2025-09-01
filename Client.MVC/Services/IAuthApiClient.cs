using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.Results;

namespace Client.MVC.Services
{
    /// <summary>
    /// Authentication API client for user authentication operations
    /// </summary>
    public interface IAuthApiClient
    {
        /// <summary>
        /// Register a new user
        /// </summary>
        Task<ApiResponse<AuthResultDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Login user
        /// </summary>
        Task<ApiResponse<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logout user
        /// </summary>
        Task<ApiResponse<LogoutResultDto>> LogoutAsync(LogoutDto? logoutDto = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refresh access token
        /// </summary>
        Task<ApiResponse<AuthResultDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
} 