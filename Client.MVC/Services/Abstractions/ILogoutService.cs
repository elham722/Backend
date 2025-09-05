using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs.Auth;

namespace Client.MVC.Services.Abstractions
{
    /// <summary>
    /// Service for handling user logout operations at API level
    /// Separate from SessionManager to avoid circular dependencies
    /// </summary>
    public interface ILogoutService
    {
        /// <summary>
        /// Logout user from API and clear local session
        /// </summary>
        /// <param name="logoutFromAllDevices">Whether to logout from all devices</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Logout result with success status and details</returns>
        Task<ApiResponse<LogoutResultDto>> LogoutAsync(bool logoutFromAllDevices = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logout only from local session (no API call)
        /// </summary>
        /// <returns>Success result</returns>
        Task<ApiResponse<LogoutResultDto>> LogoutLocalAsync();
    }
}