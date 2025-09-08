using Microsoft.AspNetCore.Authorization;

namespace Backend.Application.Common.Authorization;

/// <summary>
/// Authorization policies for Role-Based Access Control (RBAC)
/// Defines different access levels and restrictions
/// </summary>
public static class RoleBasedAuthorizationPolicies
{
    // Basic role policies
    public const string RequireUser = "RequireUser";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireSuperAdmin = "RequireSuperAdmin";

    // Admin management policies
    public const string CanManageUsers = "CanManageUsers";
    public const string CanManageRoles = "CanManageRoles";
    public const string CanManagePermissions = "CanManagePermissions";
    public const string CanManageAdminRoles = "CanManageAdminRoles";

    // Content management policies
    public const string CanManageContent = "CanManageContent";
    public const string CanViewAnalytics = "CanViewAnalytics";
    public const string CanManageSettings = "CanManageSettings";

    // System administration policies
    public const string CanManageSystem = "CanManageSystem";
    public const string CanAccessAuditLogs = "CanAccessAuditLogs";
    public const string CanManageSecurity = "CanManageSecurity";
}

/// <summary>
/// Authorization requirements for role-based access control
/// </summary>
public class RoleBasedRequirement : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; }
    public string[] RestrictedRoles { get; }
    public bool RequireAllRoles { get; }

    public RoleBasedRequirement(string[] allowedRoles, string[]? restrictedRoles = null, bool requireAllRoles = false)
    {
        AllowedRoles = allowedRoles ?? throw new ArgumentNullException(nameof(allowedRoles));
        RestrictedRoles = restrictedRoles ?? Array.Empty<string>();
        RequireAllRoles = requireAllRoles;
    }
}

/// <summary>
/// Authorization handler for role-based requirements
/// </summary>
public class RoleBasedAuthorizationHandler : AuthorizationHandler<RoleBasedRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleBasedRequirement requirement)
    {
        var userRoles = context.User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Check if user has any of the allowed roles
        var hasAllowedRole = requirement.AllowedRoles.Any(role => userRoles.Contains(role));
        
        // Check if user has any restricted roles
        var hasRestrictedRole = requirement.RestrictedRoles.Any(role => userRoles.Contains(role));

        // If user has restricted roles, deny access
        if (hasRestrictedRole)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // If require all roles, check if user has all of them
        if (requirement.RequireAllRoles)
        {
            var hasAllRoles = requirement.AllowedRoles.All(role => userRoles.Contains(role));
            if (hasAllRoles)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
        else
        {
            // If user has any allowed role, grant access
            if (hasAllowedRole)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        return Task.CompletedTask;
    }
}