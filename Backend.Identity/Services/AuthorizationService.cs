using Microsoft.AspNetCore.Identity;
using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Models;
using Backend.Application.Common.DTOs.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.Identity.Context;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Service for authorization checks and role/permission management
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly BackendIdentityDbContext _context;

        public AuthorizationService(
            UserManager<ApplicationUser> userManager,
            RoleManager<Role> roleManager,
            BackendIdentityDbContext context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> HasPermissionAsync(string userId, string resource, string action, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(resource) || string.IsNullOrEmpty(action))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any())
                return false;

            // Check if any role has the permission
            var hasPermission = await _context.RolePermissions
                .AnyAsync(rp => userRoles.Contains(rp.Role.Name) && rp.Permission.Resource == resource && rp.Permission.Action == action, cancellationToken);

            return hasPermission;
        }

        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<(string Resource, string Action)> permissions, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId) || !permissions.Any())
                return false;

            foreach (var (resource, action) in permissions)
            {
                if (await HasPermissionAsync(userId, resource, action, cancellationToken))
                    return true;
            }

            return false;
        }

        public async Task<bool> HasAllPermissionsAsync(string userId, IEnumerable<(string Resource, string Action)> permissions, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId) || !permissions.Any())
                return false;

            foreach (var (resource, action) in permissions)
            {
                if (!await HasPermissionAsync(userId, resource, action, cancellationToken))
                    return false;
            }

            return true;
        }

        public async Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return Enumerable.Empty<PermissionDto>();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Enumerable.Empty<PermissionDto>();

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any())
                return Enumerable.Empty<PermissionDto>();

            var permissions = await _context.RolePermissions
                .Where(rp => userRoles.Contains(rp.Role.Name))
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name,
                    Resource = rp.Permission.Resource,
                    Action = rp.Permission.Action,
                    Description = rp.Permission.Description,
                    CreatedAt = rp.Permission.CreatedAt,
                    UpdatedAt = rp.Permission.UpdatedAt,
                    CreatedBy = rp.Permission.CreatedBy,
                    UpdatedBy = rp.Permission.UpdatedBy,
                    IsActive = rp.Permission.IsActive,
                    IsSystemPermission = rp.Permission.IsSystemPermission,
                    Category = rp.Permission.Category,
                    Priority = rp.Permission.Priority
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            return permissions;
        }

        public async Task<bool> IsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> IsInAnyRoleAsync(string userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId) || !roleNames.Any())
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            return roleNames.Any(roleName => userRoles.Contains(roleName));
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return Enumerable.Empty<string>();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Enumerable.Empty<string>();

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> IsResourceOwnerAsync(string userId, string resourceType, string resourceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(resourceId) || string.IsNullOrEmpty(resourceType))
                return false;

            // This is a generic implementation - specific resource ownership logic should be implemented per resource type
            // For now, we'll check if the user has a specific permission for resource ownership
            var hasOwnershipPermission = await HasPermissionAsync(userId, resourceType.ToLower(), "own", cancellationToken);
            
            if (hasOwnershipPermission)
            {
                // Additional logic can be added here to check actual ownership
                // For example, checking if the resource belongs to the user
                return true;
            }

            return false;
        }

        public async Task<bool> HasBranchAccessAsync(string userId, string branchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(branchId))
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Check if user's branch matches the target branch
            var userBranchId = user.Account.BranchId;
            if (userBranchId == branchId)
                return true;

            // Check if user has cross-branch access permission
            return await HasPermissionAsync(userId, "branch", "cross", cancellationToken);
        }

        public async Task<string?> GetUserBranchIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            return user?.Account.BranchId;
        }
    }
}