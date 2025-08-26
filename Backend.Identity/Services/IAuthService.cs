using Backend.Identity.DTOs;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Interface for authentication operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        Task<AuthResult> LoginAsync(string userName, string password, bool rememberMe);

        /// <summary>
        /// Registers a new user
        /// </summary>
        Task<AuthResult> RegisterAsync(string userName, string email, string password, string? phoneNumber);

        /// <summary>
        /// Logs out the current user
        /// </summary>
        Task<bool> LogoutAsync();

        /// <summary>
        /// Refreshes the access token using a refresh token
        /// </summary>
        Task<AuthResult> RefreshTokenAsync(string refreshToken, string userId);

        /// <summary>
        /// Gets the current user profile
        /// </summary>
        Task<UserProfile?> GetUserProfileAsync(string userId);

        /// <summary>
        /// Gets the current user roles
        /// </summary>
        Task<IEnumerable<string>?> GetUserRolesAsync(string userId);
    }
} 