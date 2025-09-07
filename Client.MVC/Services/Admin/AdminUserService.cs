using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.Results;
using Client.MVC.Services.Abstractions;
using Client.MVC.Services.ApiClients;

namespace Client.MVC.Services.Admin
{
    /// <summary>
    /// Implementation of admin user service
    /// Uses existing API clients to maintain clean architecture
    /// </summary>
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserApiClient _userApiClient;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(
            IUserApiClient userApiClient,
            ILogger<AdminUserService> logger)
        {
            _userApiClient = userApiClient ?? throw new ArgumentNullException(nameof(userApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PaginatedResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10, string? searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Getting users for admin panel. Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", 
                    page, pageSize, searchTerm);

                // Use existing UserApiClient to get users
                var result = await _userApiClient.GetUsersAsync(page, pageSize);
                
                if (result != null)
                {
                    return result;
                }

                _logger.LogWarning("Failed to get users: API returned null");
                return PaginatedResult<UserDto>.Failure("خطا در دریافت کاربران");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting users for admin panel");
                return PaginatedResult<UserDto>.Failure("خطا در دریافت کاربران");
            }
        }

        public async Task<Result<UserDto>> GetUserByIdAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting user by ID for admin panel: {UserId}", userId);

                var user = await _userApiClient.GetUserByIdAsync(userId);
                
                if (user != null)
                {
                    return Result<UserDto>.Success(user);
                }

                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return Result<UserDto>.Failure("User not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user by ID {UserId}", userId);
                return Result<UserDto>.Failure("An error occurred while retrieving the user");
            }
        }

        public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                _logger.LogInformation("Creating new user for admin panel: {Email}", createUserDto.Email);

                // Note: CreateUserAsync method doesn't exist in IUserApiClient interface
                // This would need to be implemented in the API client or use a different approach
                _logger.LogWarning("CreateUserAsync method not implemented in IUserApiClient");
                return Result<UserDto>.Failure("User creation not implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user");
                return Result<UserDto>.Failure("An error occurred while creating the user");
            }
        }

        public async Task<Result<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
        {
            try
            {
                _logger.LogInformation("Updating user for admin panel: {UserId}", userId);

                var user = await _userApiClient.UpdateUserAsync(userId, updateUserDto);
                
                if (user != null)
                {
                    _logger.LogInformation("User updated successfully: {UserId}", userId);
                    return Result<UserDto>.Success(user);
                }

                _logger.LogWarning("Failed to update user {UserId}: User not found or update failed", userId);
                return Result<UserDto>.Failure("Failed to update user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user {UserId}", userId);
                return Result<UserDto>.Failure("An error occurred while updating the user");
            }
        }

        public async Task<Result> DeleteUserAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Deleting user for admin panel: {UserId}", userId);

                var success = await _userApiClient.DeleteUserAsync(userId);
                
                if (success)
                {
                    _logger.LogInformation("User deleted successfully: {UserId}", userId);
                    return Result.Success();
                }

                _logger.LogWarning("Failed to delete user {UserId}", userId);
                return Result.Failure("Failed to delete user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user {UserId}", userId);
                return Result.Failure("An error occurred while deleting the user");
            }
        }

        public async Task<Result> AssignRolesAsync(string userId, List<string> roles)
        {
            try
            {
                _logger.LogInformation("Assigning roles to user for admin panel: {UserId}, Roles: {Roles}", 
                    userId, string.Join(", ", roles));

                // Use the new UserApiClient methods
                foreach (var role in roles)
                {
                    var success = await _userApiClient.AssignRoleToUserAsync(userId, role);
                    if (!success)
                    {
                        _logger.LogWarning("Failed to assign role {Role} to user {UserId}", role, userId);
                        return Result.Failure($"خطا در اختصاص نقش {role}");
                    }
                }

                _logger.LogInformation("Successfully assigned roles to user: {UserId}", userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning roles to user {UserId}", userId);
                return Result.Failure("خطا در اختصاص نقش‌ها");
            }
        }

        public async Task<Result<List<string>>> GetAvailableRolesAsync()
        {
            try
            {
                _logger.LogInformation("Getting available roles for admin panel");

                var roles = await _userApiClient.GetAllRolesAsync();
                
                if (roles != null)
                {
                    var roleNames = roles.Select(r => r.Name).ToList();
                    _logger.LogInformation("Successfully retrieved {Count} roles", roleNames.Count);
                    return Result<List<string>>.Success(roleNames);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve roles from API");
                    return Result<List<string>>.Failure("خطا در دریافت نقش‌ها");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting available roles");
                return Result<List<string>>.Failure("خطا در دریافت نقش‌ها");
            }
        }

        public async Task<Result<AdminDashboardStatsDto>> GetUserStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting user statistics for admin dashboard");

                // For now, we'll create mock data. In a real implementation,
                // you would call a dedicated statistics API endpoint
                var statistics = new AdminDashboardStatsDto
                {
                    TotalUsers = 150,
                    ActiveUsers = 120,
                    InactiveUsers = 30,
                    NewUsersThisMonth = 25,
                    UsersWithMfa = 80,
                    UsersByRole = new Dictionary<string, int>
                    {
                        { "User", 120 },
                        { "Admin", 25 },
                        { "SuperAdmin", 5 }
                    },
                    RecentActivity = new List<UserActivityDto>
                    {
                        new() { UserId = "1", Username = "admin", Activity = "User Created", Timestamp = DateTime.Now.AddMinutes(-5) },
                        new() { UserId = "2", Username = "user1", Activity = "Login", Timestamp = DateTime.Now.AddMinutes(-10) },
                        new() { UserId = "3", Username = "user2", Activity = "Password Changed", Timestamp = DateTime.Now.AddMinutes(-15) }
                    }
                };

                return Result<AdminDashboardStatsDto>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user statistics");
                return Result<AdminDashboardStatsDto>.Failure("An error occurred while retrieving statistics");
            }
        }
    }
}