namespace Backend.Application.Common.Interfaces;

/// <summary>
/// Service for getting current user information
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user ID
    /// </summary>
    string? UserId { get; }
    
    /// <summary>
    /// Gets the current user email
    /// </summary>
    string? UserEmail { get; }
    
    /// <summary>
    /// Gets the current user roles
    /// </summary>
    IEnumerable<string> UserRoles { get; }
    
    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Checks if the current user has a specific role
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role</returns>
    bool IsInRole(string role);
    
    /// <summary>
    /// Checks if the current user has any of the specified roles
    /// </summary>
    /// <param name="roles">Roles to check</param>
    /// <returns>True if user has any of the roles</returns>
    bool IsInAnyRole(params string[] roles);
} 