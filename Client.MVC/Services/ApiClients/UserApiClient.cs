using Backend.Application.Common.Results;
using Backend.Application.Common.DTOs;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Backend.Application.Common.DTOs.Identity;
using Microsoft.Extensions.Logging;
using Client.MVC.Services.Abstractions;
using Client.MVC.ViewModels.Admin;

namespace Client.MVC.Services.ApiClients
{
    /// <summary>
    /// Implementation of user API client using authenticated HTTP client
    /// </summary>
    public class UserApiClient : IUserApiClient
    {
        private readonly IAuthenticatedHttpClient _httpClient;
        private readonly ILogger<UserApiClient> _logger;

        public UserApiClient(IAuthenticatedHttpClient httpClient, ILogger<UserApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get user profile
        /// </summary>
        public async Task<UserDto?> GetUserProfileAsync()
        {
            try
            {
                _logger.LogDebug("Getting user profile");
                
                var response = await _httpClient.GetAsync<UserDto>("api/v1.0/users/profile");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("User profile retrieved successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve user profile: {Error}", response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return null;
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            try
            {
                _logger.LogDebug("Getting user by ID: {UserId}", userId);
                
                var response = await _httpClient.GetAsync<UserDto>($"api/v1.0/users/{userId}");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("User retrieved successfully: {UserId}", userId);
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve user: {UserId}, Error: {Error}", userId, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        public async Task<PaginatedResult<UserDto>?> GetUsersAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogDebug("Getting users with pagination: Page {Page}, Size {PageSize}", page, pageSize);
                
                var response = await _httpClient.GetAsync<PaginationResponse<UserDto>>($"api/v1.0/users?page={page}&pageSize={pageSize}");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Users retrieved successfully: {Count} users", response.Data.Meta.TotalCount);
                    
                    // Convert PaginationResponse to PaginatedResult for MVC compatibility
                    return PaginatedResult<UserDto>.Success(
                        response.Data.Data,
                        response.Data.Meta.TotalCount,
                        response.Data.Meta.PageNumber,
                        response.Data.Meta.PageSize);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve users: {Error}", response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with pagination");
                return null;
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        public async Task<UserDto?> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
        {
            try
            {
                _logger.LogDebug("Updating user: {UserId}", userId);
                
                var response = await _httpClient.PutAsync<UpdateUserDto, UserDto>($"api/v1.0/users/{userId}", updateUserDto);
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogInformation("User updated successfully: {UserId}", userId);
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to update user: {UserId}, Error: {Error}", userId, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                _logger.LogDebug("Changing user password");
                
                var response = await _httpClient.PostAsync<ChangePasswordDto, object>("api/v1.0/users/change-password", changePasswordDto);
                
                if (response.IsSuccess)
                {
                    _logger.LogInformation("Password changed successfully");
                    return true;
                }
                else
                {
                    var errorMessage = response.ErrorMessage ?? "Password change failed";
                    _logger.LogWarning("Failed to change password: {Error}", errorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return false;
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                _logger.LogDebug("Deleting user: {UserId}", userId);
                
                var response = await _httpClient.DeleteAsync($"api/v1.0/users/{userId}");
                
                if (response.IsSuccess && response.Data)
                {
                    _logger.LogInformation("User deleted successfully: {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete user: {UserId}, Error: {Error}", userId, response.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Activate user
        /// </summary>
        public async Task<bool> ActivateUserAsync(string userId)
        {
            try
            {
                _logger.LogDebug("Activating user: {UserId}", userId);
                
                var response = await _httpClient.PostAsync<object, object>($"api/v1.0/users/{userId}/activate", new { });
                
                if (response.IsSuccess)
                {
                    _logger.LogInformation("User activated successfully: {UserId}", userId);
                    return true;
                }
                else
                {
                    var errorMessage = response.ErrorMessage ?? "User activation failed";
                    _logger.LogWarning("Failed to activate user: {UserId}. Error: {Error}", userId, errorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Deactivate user
        /// </summary>
        public async Task<bool> DeactivateUserAsync(string userId)
        {
            try
            {
                _logger.LogDebug("Deactivating user: {UserId}", userId);
                
                var result = await _httpClient.PostAsync<object, object>($"api/v1.0/users/{userId}/deactivate", new { });
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("User deactivated successfully: {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to deactivate user: {UserId}. Error: {Error}", userId, result?.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
                return false;
            }
        }

        // Role Management Methods
        public async Task<IEnumerable<RoleDto>?> GetAllRolesAsync()
        {
            try
            {
                _logger.LogDebug("Getting all roles");
                
                var response = await _httpClient.GetAsync<IEnumerable<RoleDto>>("api/v1/auth/roles/all");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Roles retrieved successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve roles: {Error}", response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return null;
            }
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string roleId)
        {
            try
            {
                _logger.LogDebug("Getting role by ID: {RoleId}", roleId);
                
                var response = await _httpClient.GetAsync<RoleDto>($"api/v1/auth/roles/{roleId}");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Role retrieved successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve role: {RoleId}. Error: {Error}", roleId, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role: {RoleId}", roleId);
                return null;
            }
        }

        public async Task<RoleDto?> CreateRoleAsync(CreateRoleRequest request)
        {
            try
            {
                _logger.LogDebug("Creating new role: {RoleName}", request.Name);
                
                var response = await _httpClient.PostAsync<CreateRoleRequest, RoleDto>("api/v1/auth/roles", request);
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Role created successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to create role: {RoleName}. Error: {Error}", request.Name, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", request.Name);
                return null;
            }
        }

        public async Task<RoleDto?> UpdateRoleAsync(string roleId, UpdateRoleRequest request)
        {
            try
            {
                _logger.LogDebug("Updating role: {RoleId}", roleId);
                
                var response = await _httpClient.PutAsync<UpdateRoleRequest, RoleDto>($"api/v1/auth/roles/{roleId}", request);
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Role updated successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to update role: {RoleId}. Error: {Error}", roleId, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleId}", roleId);
                return null;
            }
        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            try
            {
                _logger.LogDebug("Deleting role: {RoleId}", roleId);
                
                var result = await _httpClient.DeleteAsync($"api/v1/auth/roles/{roleId}");
                
                if (result.IsSuccess)
                {
                    _logger.LogDebug("Role deleted successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete role: {RoleId}. Error: {Error}", roleId, result.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
                return false;
            }
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
        {
            try
            {
                _logger.LogDebug("Assigning role to user: {UserId}, Role: {RoleName}", userId, roleName);
                
                var request = new AssignRoleRequest
                {
                    UserId = userId,
                    RoleName = roleName
                };
                
                var result = await _httpClient.PostAsync<AssignRoleRequest, object>($"api/v1/auth/assign-role", request);
                
                if (result.IsSuccess)
                {
                    _logger.LogDebug("Role assigned successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to assign role: {UserId}, {RoleName}. Error: {Error}", userId, roleName, result.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role: {UserId}, {RoleName}", userId, roleName);
                return false;
            }
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            try
            {
                _logger.LogDebug("Removing role from user: {UserId}, Role: {RoleName}", userId, roleName);
                
                var request = new RemoveRoleRequest
                {
                    UserId = userId,
                    RoleName = roleName
                };
                
                var result = await _httpClient.PostAsync<RemoveRoleRequest, object>($"api/v1/auth/remove-role", request);
                
                if (result.IsSuccess)
                {
                    _logger.LogDebug("Role removed successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to remove role: {UserId}, {RoleName}. Error: {Error}", userId, roleName, result.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role: {UserId}, {RoleName}", userId, roleName);
                return false;
            }
        }

        public async Task<IEnumerable<string>?> GetUserRolesAsync(string userId)
        {
            try
            {
                _logger.LogDebug("Getting user roles: {UserId}", userId);
                
                var response = await _httpClient.GetAsync<IEnumerable<string>>($"api/v1/auth/users/{userId}/roles");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("User roles retrieved successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve user roles: {UserId}. Error: {Error}", userId, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles: {UserId}", userId);
                return null;
            }
        }

        // Permission Management Methods
        public async Task<IEnumerable<PermissionDto>?> GetAllPermissionsAsync()
        {
            try
            {
                _logger.LogDebug("Getting all permissions");
                
                var response = await _httpClient.GetAsync<IEnumerable<PermissionDto>>("api/v1/auth/permissions/all");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Permissions retrieved successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve permissions: {Error}", response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions");
                return null;
            }
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(string permissionId)
        {
            try
            {
                _logger.LogDebug("Getting permission by ID: {PermissionId}", permissionId);
                
                var response = await _httpClient.GetAsync<PermissionDto>($"api/v1/auth/permissions/{permissionId}");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Permission retrieved successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve permission: {PermissionId}. Error: {Error}", permissionId, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission: {PermissionId}", permissionId);
                return null;
            }
        }

        public async Task<PermissionDto?> CreatePermissionAsync(CreatePermissionRequest request)
        {
            try
            {
                _logger.LogDebug("Creating new permission: {PermissionName}", request.Name);
                
                var response = await _httpClient.PostAsync<CreatePermissionRequest, PermissionDto>("api/v1/auth/permissions", request);
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Permission created successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to create permission: {PermissionName}. Error: {Error}", request.Name, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission: {PermissionName}", request.Name);
                return null;
            }
        }

        public async Task<PermissionDto?> UpdatePermissionAsync(string permissionId, UpdatePermissionRequest request)
        {
            try
            {
                _logger.LogDebug("Updating permission: {PermissionId}", permissionId);
                
                var response = await _httpClient.PutAsync<UpdatePermissionRequest, PermissionDto>($"api/v1/auth/permissions/{permissionId}", request);
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Permission updated successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to update permission: {PermissionId}. Error: {Error}", permissionId, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission: {PermissionId}", permissionId);
                return null;
            }
        }

        public async Task<bool> DeletePermissionAsync(string permissionId)
        {
            try
            {
                _logger.LogDebug("Deleting permission: {PermissionId}", permissionId);
                
                var result = await _httpClient.DeleteAsync($"api/v1/auth/permissions/{permissionId}");
                
                if (result.IsSuccess)
                {
                    _logger.LogDebug("Permission deleted successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete permission: {PermissionId}. Error: {Error}", permissionId, result.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission: {PermissionId}", permissionId);
                return false;
            }
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId)
        {
            try
            {
                _logger.LogDebug("Assigning permission to role: {RoleId}, Permission: {PermissionId}", roleId, permissionId);
                
                var request = new AssignPermissionRequest();
                
                var result = await _httpClient.PostAsync<AssignPermissionRequest, object>($"api/v1/auth/roles/{roleId}/permissions/{permissionId}", request);
                
                if (result.IsSuccess)
                {
                    _logger.LogDebug("Permission assigned successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to assign permission: {RoleId}, {PermissionId}. Error: {Error}", roleId, permissionId, result.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permission: {RoleId}, {PermissionId}", roleId, permissionId);
                return false;
            }
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
        {
            try
            {
                _logger.LogDebug("Removing permission from role: {RoleId}, Permission: {PermissionId}", roleId, permissionId);
                
                var result = await _httpClient.DeleteAsync($"api/v1/auth/roles/{roleId}/permissions/{permissionId}");
                
                if (result.IsSuccess)
                {
                    _logger.LogDebug("Permission removed successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to remove permission: {RoleId}, {PermissionId}. Error: {Error}", roleId, permissionId, result.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing permission: {RoleId}, {PermissionId}", roleId, permissionId);
                return false;
            }
        }

        public async Task<IEnumerable<PermissionDto>?> GetRolePermissionsAsync(string roleId)
        {
            try
            {
                _logger.LogDebug("Getting role permissions: {RoleId}", roleId);
                
                var response = await _httpClient.GetAsync<IEnumerable<PermissionDto>>($"api/v1/auth/roles/{roleId}/permissions");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Role permissions retrieved successfully");
                    return response.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve role permissions: {RoleId}. Error: {Error}", roleId, response.ErrorMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role permissions: {RoleId}", roleId);
                return null;
            }
        }
    }
} 