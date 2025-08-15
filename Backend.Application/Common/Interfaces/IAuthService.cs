using Backend.Application.Features.Auth.DTOs;

namespace Backend.Application.Common.Interfaces
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
    }

    /// <summary>
    /// Result of authentication operations
    /// </summary>
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<string>? Roles { get; set; }
        public List<string>? Errors { get; set; }
        public bool IsLockedOut { get; set; }
        public bool RequiresTwoFactor { get; set; }
    }

    /// <summary>
    /// User profile information
    /// </summary>
    public class UserProfile
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
} 