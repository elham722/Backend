using Backend.Application.Common.DTOs.Identity;

namespace Backend.Application.Common.Interfaces.Identity
{
    /// <summary>
    /// Service interface for authorization operations
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks if user has a specific permission
        /// </summary>
        Task<bool> HasPermissionAsync(string userId, string resource, string action, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if user has any of the specified permissions
        /// </summary>
        Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<(string Resource, string Action)> permissions, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if user has all of the specified permissions
        /// </summary>
        Task<bool> HasAllPermissionsAsync(string userId, IEnumerable<(string Resource, string Action)> permissions, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all permissions for a user
        /// </summary>
        Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if user is in a specific role
        /// </summary>
        Task<bool> IsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if user is in any of the specified roles
        /// </summary>
        Task<bool> IsInAnyRoleAsync(string userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all roles for a user
        /// </summary>
        Task<IEnumerable<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if user owns a specific resource
        /// </summary>
        Task<bool> IsResourceOwnerAsync(string userId, string resourceType, string resourceId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if user has access to a specific branch
        /// </summary>
        Task<bool> HasBranchAccessAsync(string userId, string branchId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets user's branch ID
        /// </summary>
        Task<string?> GetUserBranchIdAsync(string userId, CancellationToken cancellationToken = default);
    }
}