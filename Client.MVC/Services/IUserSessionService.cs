using Backend.Application.Features.UserManagement.DTOs;

namespace Client.MVC.Services
{
    /// <summary>
    /// Service for managing user session data
    /// </summary>
    public interface IUserSessionService
    {
        /// <summary>
        /// Set user session data from authentication result
        /// </summary>
        /// <param name="result">Authentication result containing user data and tokens</param>
        void SetUserSession(AuthResultDto result);

        /// <summary>
        /// Clear all user session data
        /// </summary>
        void ClearUserSession();

        /// <summary>
        /// Clear all user session data asynchronously (recommended)
        /// </summary>
        Task ClearUserSessionAsync();

        /// <summary>
        /// Logout user with options
        /// </summary>
        /// <param name="logoutFromAllDevices">Whether to logout from all devices</param>
        /// <returns>True if logout was successful, false otherwise</returns>
        Task<bool> LogoutAsync(bool logoutFromAllDevices = false);

        /// <summary>
        /// Get current user ID from session
        /// </summary>
        /// <returns>User ID if exists, null otherwise</returns>
        string? GetUserId();

        /// <summary>
        /// Get current user name from session
        /// </summary>
        /// <returns>User name if exists, null otherwise</returns>
        string? GetUserName();

        /// <summary>
        /// Get current user email from session
        /// </summary>
        /// <returns>User email if exists, null otherwise</returns>
        string? GetUserEmail();

        /// <summary>
        /// Get JWT token from HttpOnly cookie
        /// </summary>
        /// <returns>JWT token if exists, null otherwise</returns>
        string? GetJwtToken();

        /// <summary>
        /// Get refresh token from HttpOnly cookie
        /// </summary>
        /// <returns>Refresh token if exists, null otherwise</returns>
        string? GetRefreshToken();

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        /// <returns>True if user is authenticated, false otherwise</returns>
        bool IsAuthenticated();

        /// <summary>
        /// Check if JWT token is about to expire (within specified minutes)
        /// </summary>
        /// <param name="minutesBeforeExpiry">Minutes before expiry to consider token as "about to expire"</param>
        /// <returns>True if token expires within specified minutes, false otherwise</returns>
        bool IsTokenAboutToExpire(int minutesBeforeExpiry = 5);

        /// <summary>
        /// Get JWT token expiration time
        /// </summary>
        /// <returns>Token expiration time if valid, null otherwise</returns>
        DateTime? GetTokenExpiration();

        /// <summary>
        /// Check if refresh token exists and is valid
        /// </summary>
        /// <returns>True if refresh token exists, false otherwise</returns>
        bool HasValidRefreshToken();

        /// <summary>
        /// Get refresh token expiration time (if it's a JWT token)
        /// </summary>
        /// <returns>Refresh token expiration time if it's a JWT, null otherwise</returns>
        DateTime? GetRefreshTokenExpiration();

        /// <summary>
        /// Check if refresh token is about to expire (within specified days)
        /// </summary>
        /// <param name="daysBeforeExpiry">Days before expiry to consider token as "about to expire"</param>
        /// <returns>True if token expires within specified days, false otherwise</returns>
        bool IsRefreshTokenAboutToExpire(int daysBeforeExpiry = 7);

        /// <summary>
        /// Get refresh token type (JWT or Opaque)
        /// </summary>
        /// <returns>Token type if valid, null otherwise</returns>
        string? GetRefreshTokenType();

        /// <summary>
        /// Get logout DTO with current refresh token
        /// </summary>
        /// <returns>Logout DTO with refresh token if available</returns>
        LogoutDto GetLogoutDto();

        /// <summary>
        /// Refresh JWT token cookie (called after token refresh)
        /// </summary>
        /// <param name="newToken">New JWT token</param>
        /// <param name="expiresAt">Token expiration time</param>
        void RefreshJwtToken(string newToken, DateTime? expiresAt = null);
    }
} 