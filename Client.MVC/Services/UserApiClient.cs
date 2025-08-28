using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;
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
                
                var result = await _httpClient.GetAsync<UserProfileDto>("api/User/profile");
                
                if (result != null)
                {
                    _logger.LogDebug("User profile retrieved successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve user profile");
                }
                
                return result;
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
                
                var result = await _httpClient.GetAsync<UserDto>($"api/User/{userId}");
                
                if (result != null)
                {
                    _logger.LogDebug("User retrieved successfully: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve user: {UserId}", userId);
                }
                
                return result;
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
                
                var result = await _httpClient.GetAsync<PaginatedResult<UserDto>>($"api/User?page={page}&pageSize={pageSize}");
                
                if (result != null)
                {
                    _logger.LogDebug("Users retrieved successfully: {Count} users", result.TotalCount);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve users");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with pagination");
                return null;
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        public async Task<UserDto?> UpdateUserAsync(string userId, UpdateUserDto updateDto)
        {
            try
            {
                _logger.LogDebug("Updating user: {UserId}", userId);
                
                var result = await _httpClient.PutAsync<UpdateUserDto, UserDto>($"api/User/{userId}", updateDto);
                
                if (result != null)
                {
                    _logger.LogInformation("User updated successfully: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to update user: {UserId}", userId);
                }
                
                return result;
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
                
                var result = await _httpClient.PostAsync<ChangePasswordDto, AuthResultDto>("api/User/change-password", changePasswordDto);
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("Password changed successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to change password: {Error}", result?.ErrorMessage);
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
                
                var result = await _httpClient.DeleteAsync($"api/User/{userId}");
                
                if (result)
                {
                    _logger.LogInformation("User deleted successfully: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to delete user: {UserId}", userId);
                }
                
                return result;
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
                
                var result = await _httpClient.PostAsync<object, AuthResultDto>($"api/User/{userId}/activate", new { });
                
                if (result?.IsSuccess == true)
                {
                    _logger.LogInformation("User activated successfully: {UserId}", userId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to activate user: {UserId}. Error: {Error}", userId, result?.ErrorMessage);
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
                
                var result = await _httpClient.PostAsync<object, AuthResultDto>($"api/User/{userId}/deactivate", new { });
                
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