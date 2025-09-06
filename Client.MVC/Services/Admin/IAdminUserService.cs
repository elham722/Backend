using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Common.Results;

namespace Client.MVC.Services.Admin
{
    /// <summary>
    /// Service interface for admin user management operations
    /// </summary>
    public interface IAdminUserService
    {
        /// <summary>
        /// Gets all users with pagination
        /// </summary>
        Task<PaginatedResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10, string? searchTerm = null);

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        Task<Result<UserDto>> GetUserByIdAsync(string userId);

        /// <summary>
        /// Creates a new user
        /// </summary>
        Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        Task<Result<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);

        /// <summary>
        /// Deletes a user
        /// </summary>
        Task<Result> DeleteUserAsync(string userId);

        /// <summary>
        /// Assigns roles to a user
        /// </summary>
        Task<Result> AssignRolesAsync(string userId, List<string> roles);

        /// <summary>
        /// Gets all available roles
        /// </summary>
        Task<Result<List<string>>> GetAvailableRolesAsync();

        /// <summary>
        /// Gets user statistics for dashboard
        /// </summary>
        Task<Result<AdminDashboardStatsDto>> GetUserStatisticsAsync();
    }

    /// <summary>
    /// DTO for admin dashboard statistics
    /// </summary>
    public class AdminDashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int UsersWithMfa { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new();
        public List<UserActivityDto> RecentActivity { get; set; } = new();
    }

    /// <summary>
    /// DTO for user activity
    /// </summary>
    public class UserActivityDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Activity { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }
}