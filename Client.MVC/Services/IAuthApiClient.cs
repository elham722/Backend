using Backend.Application.Features.UserManagement.DTOs;

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
        Task<AuthResultDto> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Login user
        /// </summary>
        Task<AuthResultDto> LoginAsync(LoginDto dto);

        /// <summary>
        /// Logout user
        /// </summary>
        Task<bool> LogoutAsync(LogoutDto? logoutDto = null);

        /// <summary>
        /// Refresh access token
        /// </summary>
        Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
    }
} 