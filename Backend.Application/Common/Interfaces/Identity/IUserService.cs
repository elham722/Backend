using Backend.Application.Common.DTOs.Identity;

namespace Backend.Application.Common.Interfaces.Identity
{
    /// <summary>
    /// Service interface for authorization operations
    /// </summary>
    public interface IUserService
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
        
        /// <summary>
        /// Assigns a role to a user
        /// </summary>
        Task<bool> AssignRoleAsync(string userId, string roleName, string assignedBy, DateTime? expiresAt = null, string? reason = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Removes a role from a user
        /// </summary>
        Task<bool> RemoveRoleAsync(string userId, string roleName, string removedBy, string? reason = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Assigns a permission to a role
        /// </summary>
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string assignedBy, DateTime? expiresAt = null, string? reason = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Removes a permission from a role
        /// </summary>
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string removedBy, string? reason = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all permissions for a role
        /// </summary>
        Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new role
        /// </summary>
        Task<RoleDto> CreateRoleAsync(string name, string description, string? category = null, int priority = 0, bool isSystemRole = false, string createdBy = "System", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new permission
        /// </summary>
        Task<PermissionDto> CreatePermissionAsync(string name, string resource, string action, string description, string? category = null, int priority = 0, bool isSystemPermission = false, string createdBy = "System", CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all roles
        /// </summary>
        Task<IEnumerable<RoleDto>> GetAllRolesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all permissions
        /// </summary>
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a role by name
        /// </summary>
        Task<RoleDto?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a permission by resource and action
        /// </summary>
        Task<PermissionDto?> GetPermissionAsync(string resource, string action, CancellationToken cancellationToken = default);
    }
}