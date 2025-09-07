using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.DTOs.Identity;
using Client.MVC.ViewModels.Admin;

namespace Client.MVC.Services.Abstractions
{
    /// <summary>
    /// Typed API client for user management operations
    /// </summary>
    public interface IUserApiClient
    {
        /// <summary>
        /// Get user profile
        /// </summary>
        Task<UserDto?> GetUserProfileAsync();

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

        // Role Management Methods
        /// <summary>
        /// Get all available roles
        /// </summary>
        Task<IEnumerable<RoleDto>?> GetAllRolesAsync();

        /// <summary>
        /// Get role by ID
        /// </summary>
        Task<RoleDto?> GetRoleByIdAsync(string roleId);

        /// <summary>
        /// Create new role
        /// </summary>
        Task<RoleDto?> CreateRoleAsync(CreateRoleRequest request);

        /// <summary>
        /// Update role
        /// </summary>
        Task<RoleDto?> UpdateRoleAsync(string roleId, UpdateRoleRequest request);

        /// <summary>
        /// Delete role
        /// </summary>
        Task<bool> DeleteRoleAsync(string roleId);

        /// <summary>
        /// Assign role to user
        /// </summary>
        Task<bool> AssignRoleToUserAsync(string userId, string roleName);

        /// <summary>
        /// Remove role from user
        /// </summary>
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleName);

        /// <summary>
        /// Get user roles
        /// </summary>
        Task<IEnumerable<string>?> GetUserRolesAsync(string userId);

        // Permission Management Methods
        /// <summary>
        /// Get all available permissions
        /// </summary>
        Task<IEnumerable<PermissionDto>?> GetAllPermissionsAsync();

        /// <summary>
        /// Get permission by ID
        /// </summary>
        Task<PermissionDto?> GetPermissionByIdAsync(string permissionId);

        /// <summary>
        /// Create new permission
        /// </summary>
        Task<PermissionDto?> CreatePermissionAsync(CreatePermissionRequest request);

        /// <summary>
        /// Update permission
        /// </summary>
        Task<PermissionDto?> UpdatePermissionAsync(string permissionId, UpdatePermissionRequest request);

        /// <summary>
        /// Delete permission
        /// </summary>
        Task<bool> DeletePermissionAsync(string permissionId);

        /// <summary>
        /// Assign permission to role
        /// </summary>
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId);

        /// <summary>
        /// Remove permission from role
        /// </summary>
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);

        /// <summary>
        /// Get role permissions
        /// </summary>
        Task<IEnumerable<PermissionDto>?> GetRolePermissionsAsync(string roleId);
    }
} 