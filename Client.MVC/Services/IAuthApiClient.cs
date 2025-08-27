using Backend.Application.Features.UserManagement.DTOs;

namespace Client.MVC.Services
{
    /// <summary>
    /// Typed API client for authentication operations
    /// </summary>
    public interface IAuthApiClient
    {
        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="dto">Registration data</param>
        /// <returns>Authentication result</returns>
        Task<AuthResultDto> RegisterAsync(RegisterDto dto);

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="dto">Login data</param>
        /// <returns>Authentication result</returns>
        Task<AuthResultDto> LoginAsync(LoginDto dto);

        /// <summary>
        /// Logout user
        /// </summary>
        /// <param name="logoutDto">Logout request data</param>
        /// <returns>Success status</returns>
        Task<bool> LogoutAsync(LogoutDto? logoutDto = null);

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>New authentication result</returns>
        Task<AuthResultDto> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Validate current token
        /// </summary>
        /// <returns>Validation result</returns>
        Task<bool> ValidateTokenAsync();
    }
} 