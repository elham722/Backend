using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;

namespace Client.MVC.Services
{
    /// <summary>
    /// Typed API client for user management operations
    /// </summary>
    public interface IUserApiClient
    {
        /// <summary>
        /// Get user profile
        /// </summary>
        Task<UserProfileDto?> GetUserProfileAsync();

        /// <summary>
        /// Get user by ID
        /// </summary>
        Task<UserDto?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        Task<PaginatedResult<UserDto>?> GetUsersAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Update user profile
        /// </summary>
        Task<UserDto?> UpdateUserAsync(string userId, UpdateUserDto updateDto);

        /// <summary>
        /// Change user password
        /// </summary>
        Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);

        /// <summary>
        /// Delete user
        /// </summary>
        Task<bool> DeleteUserAsync(string userId);

        /// <summary>
        /// Activate user
        /// </summary>
        Task<bool> ActivateUserAsync(string userId);

        /// <summary>
        /// Deactivate user
        /// </summary>
        Task<bool> DeactivateUserAsync(string userId);
    }
} 