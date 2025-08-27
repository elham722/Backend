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
        /// Get JWT token from session
        /// </summary>
        /// <returns>JWT token if exists, null otherwise</returns>
        string? GetJwtToken();

        /// <summary>
        /// Get refresh token from session
        /// </summary>
        /// <returns>Refresh token if exists, null otherwise</returns>
        string? GetRefreshToken();

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        /// <returns>True if user is authenticated, false otherwise</returns>
        bool IsAuthenticated();

        /// <summary>
        /// Get logout DTO with current refresh token
        /// </summary>
        /// <returns>Logout DTO with refresh token if available</returns>
        LogoutDto GetLogoutDto();
    }
} 