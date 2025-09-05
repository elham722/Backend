using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs.Auth;

namespace Client.MVC.Services.Abstractions
{
    /// <summary>
    /// Authentication API client for user authentication operations
    /// </summary>
    public interface IAuthApiClient
    {
        /// <summary>
        /// Register a new user
        /// </summary>
        Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Login user
        /// </summary>
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logout user
        /// </summary>
        Task<ApiResponse<LogoutResultDto>> LogoutAsync(LogoutDto? logoutDto = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refresh access token
        /// </summary>
        Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
} 