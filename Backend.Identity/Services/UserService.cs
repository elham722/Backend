using Backend.Application.Common.DTOs.Identity;
using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace Backend.Identity.Services;

/// <summary>
/// Implementation of IUserService for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> HasPermissionAsync(string userId, string resource, string action, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                // Check if role has the permission
                var hasPermission = await HasRolePermissionAsync(role.Id, resource, action);
                if (hasPermission) return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for user: {UserId}, Resource: {Resource}, Action: {Action}", userId, resource, action);
            return false;
        }
    }

    public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<(string Resource, string Action)> permissions, CancellationToken cancellationToken = default)
    {
        foreach (var (resource, action) in permissions)
        {
            if (await HasPermissionAsync(userId, resource, action, cancellationToken))
                return true;
        }
        return false;
    }

    public async Task<bool> HasAllPermissionsAsync(string userId, IEnumerable<(string Resource, string Action)> permissions, CancellationToken cancellationToken = default)
    {
        foreach (var (resource, action) in permissions)
        {
            if (!await HasPermissionAsync(userId, resource, action, cancellationToken))
                return false;
        }
        return true;
    }

    public async Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Enumerable.Empty<PermissionDto>();

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<PermissionDto>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var rolePermissions = await GetRolePermissionsAsync(role.Id, cancellationToken);
                permissions.AddRange(rolePermissions);
            }

            return permissions.DistinctBy(p => new { p.Resource, p.Action });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user: {UserId}", userId);
            return Enumerable.Empty<PermissionDto>();
        }
    }

    public async Task<bool> IsInRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, roleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role for user: {UserId}, Role: {Role}", userId, roleName);
            return false;
        }
    }

    public async Task<bool> IsInAnyRoleAsync(string userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
    {
        foreach (var roleName in roleNames)
        {
            if (await IsInRoleAsync(userId, roleName, cancellationToken))
                return true;
        }
        return false;
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Enumerable.Empty<string>();

            return await _userManager.GetRolesAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user: {UserId}", userId);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> IsResourceOwnerAsync(string userId, string resourceType, string resourceId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This is a simplified implementation
            // In a real application, you would check against the actual resource
            // For now, we'll return true if the user ID matches the resource ID
            return userId == resourceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking resource ownership for user: {UserId}, ResourceType: {ResourceType}, ResourceId: {ResourceId}", userId, resourceType, resourceId);
            return false;
        }
    }

    public async Task<bool> HasBranchAccessAsync(string userId, string branchId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This is a simplified implementation
            // In a real application, you would check against the user's branch assignments
            // For now, we'll return true for all users
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking branch access for user: {UserId}, BranchId: {BranchId}", userId, branchId);
            return false;
        }
    }

    public async Task<string?> GetUserBranchIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            // This is a simplified implementation
            // In a real application, you would get the branch ID from user claims or a separate table
            return "default-branch";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch ID for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> AssignRoleAsync(string userId, string roleName, string assignedBy, DateTime? expiresAt = null, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return false;

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId}, Role: {Role}", userId, roleName);
            return false;
        }
    }

    public async Task<bool> RemoveRoleAsync(string userId, string roleName, string removedBy, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserId}, Role: {Role}", userId, roleName);
            return false;
        }
    }

    public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string assignedBy, DateTime? expiresAt = null, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // This is a simplified implementation
            // In a real application, you would add the permission to the role in the database
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permission to role: {RoleId}, Permission: {PermissionId}", roleId, permissionId);
            return false;
        }
    }

    public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string removedBy, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // This is a simplified implementation
            // In a real application, you would remove the permission from the role in the database
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission from role: {RoleId}, Permission: {PermissionId}", roleId, permissionId);
            return false;
        }
    }

    public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(string roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This is a simplified implementation
            // In a real application, you would get the permissions for the role from the database
            return Enumerable.Empty<PermissionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for role: {RoleId}", roleId);
            return Enumerable.Empty<PermissionDto>();
        }
    }

    public async Task<RoleDto> CreateRoleAsync(string name, string description, string? category = null, int priority = 0, bool isSystemRole = false, string createdBy = "System", CancellationToken cancellationToken = default)
    {
        try
        {
            var role = Role.Create(name, description, category, priority, isSystemRole, createdBy);

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return _mapper.Map<RoleDto>(role);
            }

            throw new InvalidOperationException($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role: {Name}", name);
            throw;
        }
    }

    public async Task<PermissionDto> CreatePermissionAsync(string name, string resource, string action, string description, string? category = null, int priority = 0, bool isSystemPermission = false, string createdBy = "System", CancellationToken cancellationToken = default)
    {
        try
        {
            var permission = Permission.Create(name, resource, action, description, category, priority, isSystemPermission, createdBy);

            // This is a simplified implementation
            // In a real application, you would save the permission to the database
            return _mapper.Map<PermissionDto>(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission: {Name}", name);
            throw;
        }
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = _roleManager.Roles.AsEnumerable();
            if (!includeInactive)
            {
                // Filter out inactive roles if needed
            }

            return roles.Select(role => _mapper.Map<RoleDto>(role));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all roles");
            return Enumerable.Empty<RoleDto>();
        }
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        try
        {
            // This is a simplified implementation
            // In a real application, you would get permissions from the database
            return Enumerable.Empty<PermissionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all permissions");
            return Enumerable.Empty<PermissionDto>();
        }
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            return role != null ? _mapper.Map<RoleDto>(role) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role by name: {RoleName}", roleName);
            return null;
        }
    }

    public async Task<PermissionDto?> GetPermissionAsync(string resource, string action, CancellationToken cancellationToken = default)
    {
        try
        {
            // This is a simplified implementation
            // In a real application, you would get the permission from the database
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permission: {Resource}:{Action}", resource, action);
            return null;
        }
    }

    // Helper method to check if a role has a specific permission
    private async Task<bool> HasRolePermissionAsync(string roleId, string resource, string action)
    {
        try
        {
            // This is a simplified implementation
            // In a real application, you would check the RolePermission table
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking role permission: {RoleId}, {Resource}:{Action}", roleId, resource, action);
            return false;
        }
    }
}