using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs.Auth;

namespace Client.MVC.Services.Abstractions
{
    /// <summary>
    /// Service for managing user session lifecycle
    /// Follows Single Responsibility Principle - only handles session management
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Set user session data from authentication result
        /// </summary>
        /// <param name="result">Authentication result containing user data and tokens</param>
        void SetUserSession(LoginResponse result);

        /// <summary>
        /// Set user session data from API response
        /// </summary>
        /// <param name="response">API response containing authentication result</param>
        void SetUserSession(ApiResponse<LoginResponse> response);

        /// <summary>
        /// Clear all user session data
        /// </summary>
        void ClearUserSession();

        /// <summary>
        /// Clear all user session data asynchronously (recommended)
        /// </summary>
        Task ClearUserSessionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Note: Logout operations moved to ILogoutService to avoid circular dependencies
        /// This interface now only handles local session management
        /// </summary>

        /// <summary>
        /// Get logout DTO with current refresh token
        /// </summary>
        /// <returns>Logout DTO with refresh token if available</returns>
        LogoutDto GetLogoutDto();
    }
}