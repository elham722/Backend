using Backend.Application.Common.DTOs;
using Backend.Application.Common.Results;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Application.Common.Interfaces;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user
    /// </summary>
    Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto, string createdBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing user
    /// </summary>
    Task<Result<UserDto>> UpdateUserAsync(string userId, UpdateUserDto updateUserDto, string updatedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a user (soft delete by default)
    /// </summary>
    Task<Result> DeleteUserAsync(string userId, string deletedBy, bool permanentDelete = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    Task<Result<UserDto>> GetUserByIdAsync(string userId, bool includeRoles = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a paginated list of users
    /// </summary>
    Task<Result<PaginationResponse<UserDto>>> GetUsersAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null,
        string? status = null,
        string? role = null,
        bool? emailConfirmed = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDirection = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Authenticates a user
    /// </summary>
    Task<Result<AuthResultDto>> LoginAsync(LoginDto loginDto, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<Result<AuthResultDto>> RegisterAsync(RegisterDto registerDto, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Changes user password
    /// </summary>
    Task<Result> ChangePasswordAsync(string? userId, string? currentPassword, string newPassword, string changedBy, bool requirePasswordChangeOnNextLogin = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Activates a user account
    /// </summary>
    Task<Result> ActivateUserAsync(string userId, string activatedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deactivates a user account
    /// </summary>
    Task<Result> DeactivateUserAsync(string userId, string deactivatedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resets user password (admin function)
    /// </summary>
    Task<Result> ResetPasswordAsync(string userId, string newPassword, string resetBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Assigns roles to a user
    /// </summary>
    Task<Result> AssignRolesAsync(string userId, List<string> roles, string assignedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes roles from a user
    /// </summary>
    Task<Result> RemoveRolesAsync(string userId, List<string> roles, string removedBy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets user roles
    /// </summary>
    Task<Result<List<string>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if user has a specific role
    /// </summary>
    Task<Result<bool>> UserHasRoleAsync(string userId, string role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Confirms user email
    /// </summary>
    Task<Result> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends email confirmation
    /// </summary>
    Task<Result> SendEmailConfirmationAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends password reset email
    /// </summary>
    Task<Result> SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resets password using token
    /// </summary>
    Task<Result> ResetPasswordWithTokenAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
} 