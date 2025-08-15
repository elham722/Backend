using System.Security.Claims;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Interface for policy-based authorization service
    /// </summary>
    public interface ICustomAuthorizationService
    {
        /// <summary>
        /// Checks if a user has a specific permission
        /// </summary>
        Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission);

        /// <summary>
        /// Checks if a user has any of the specified permissions
        /// </summary>
        Task<bool> HasAnyPermissionAsync(ClaimsPrincipal user, params string[] permissions);

        /// <summary>
        /// Checks if a user has all of the specified permissions
        /// </summary>
        Task<bool> HasAllPermissionsAsync(ClaimsPrincipal user, params string[] permissions);

        /// <summary>
        /// Checks if a user has a specific role
        /// </summary>
        Task<bool> HasRoleAsync(ClaimsPrincipal user, string role);

        /// <summary>
        /// Checks if a user has any of the specified roles
        /// </summary>
        Task<bool> HasAnyRoleAsync(ClaimsPrincipal user, params string[] roles);

        /// <summary>
        /// Gets all permissions for a user
        /// </summary>
        Task<IEnumerable<string>> GetUserPermissionsAsync(ClaimsPrincipal user);

        /// <summary>
        /// Gets all roles for a user
        /// </summary>
        Task<IEnumerable<string>> GetUserRolesAsync(ClaimsPrincipal user);

        /// <summary>
        /// Validates a custom policy
        /// </summary>
        Task<bool> ValidatePolicyAsync(ClaimsPrincipal user, string policyName, object? resource = null);
    }
} 