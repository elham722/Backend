using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Auth;
using Microsoft.Extensions.Logging;

namespace Client.MVC.Services
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
        public async Task<UserProfileDto?> GetUserProfileAsync()
        {
            try
            {
                _logger.LogDebug("Getting user profile");
                
                var response = await _httpClient.GetAsync<UserProfileDto>("api/User/profile");
                
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
                
                var response = await _httpClient.GetAsync<UserDto>($"api/User/{userId}");
                
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
                
                var response = await _httpClient.GetAsync<PaginatedResult<UserDto>>($"api/User?page={page}&pageSize={pageSize}");
                
                if (response.IsSuccess && response.Data != null)
                {
                    _logger.LogDebug("Users retrieved successfully: {Count} users", response.Data.TotalCount);
                    return response.Data;
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
                
                var response = await _httpClient.PutAsync<UpdateUserDto, UserDto>($"api/User/{userId}", updateUserDto);
                
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
                
                var response = await _httpClient.PostAsync<ChangePasswordDto, LoginResponse>("api/User/change-password", changePasswordDto);
                
                if (response.IsSuccess && response.Data?.IsSuccess == true)
                {
                    _logger.LogInformation("Password changed successfully");
                    return true;
                }
                else
                {
                    var errorMessage = response.Data?.ErrorMessage ?? response.ErrorMessage ?? "Password change failed";
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
                
                var response = await _httpClient.DeleteAsync($"api/User/{userId}");
                
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
                
                var response = await _httpClient.PostAsync<object, LoginResponse>($"api/User/{userId}/activate", new { });
                
                if (response.IsSuccess && response.Data?.IsSuccess == true)
                {
                    _logger.LogInformation("User activated successfully: {UserId}", userId);
                    return true;
                }
                else
                {
                    var errorMessage = response.Data?.ErrorMessage ?? response.ErrorMessage ?? "User activation failed";
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
                
                var result = await _httpClient.PostAsync<object, LoginResponse>($"api/User/{userId}/deactivate", new { });
                
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
    }
} 