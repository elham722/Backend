using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Common.Authorization;

namespace Backend.Infrastructure.Authorization;

/// <summary>
/// Service collection extensions for role-based authorization
/// This belongs in Infrastructure layer as it deals with DI container configuration
/// </summary>
public static class RoleBasedAuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddRoleBasedAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Basic role policies
            options.AddPolicy(RoleBasedAuthorizationPolicies.RequireUser, policy =>
                policy.RequireRole("User", "Admin", "SuperAdmin"));

            options.AddPolicy(RoleBasedAuthorizationPolicies.RequireAdmin, policy =>
                policy.RequireRole("Admin", "SuperAdmin"));

            options.AddPolicy(RoleBasedAuthorizationPolicies.RequireSuperAdmin, policy =>
                policy.RequireRole("SuperAdmin"));

            // Admin management policies
            options.AddPolicy(RoleBasedAuthorizationPolicies.CanManageUsers, policy =>
                policy.RequireRole("Admin", "SuperAdmin"));

            options.AddPolicy(RoleBasedAuthorizationPolicies.CanManageRoles, policy =>
                policy.RequireRole("SuperAdmin"));

            options.AddPolicy(RoleBasedAuthorizationPolicies.CanManagePermissions, policy =>
                policy.RequireRole("SuperAdmin"));

            options.AddPolicy(RoleBasedAuthorizationPolicies.CanManageAdminRoles, policy =>
                policy.RequireRole("SuperAdmin"));

            // Content management policies
            options.AddPolicy(RoleBasedAuthorizationPolicies.CanManageContent, policy =>
                policy.RequireRole("Admin", "SuperAdmin"));

            options.AddPolicy(RoleBasedAuthorizationPolicies.CanViewAnalytics, policy =>
                policy.RequireRole("Admin", "SuperAdmin"));

            options.AddPolicy(RoleBasedAuthorizationPolicies.CanManageSettings, policy =>
                policy.RequireRole("SuperAdmin"));

            // System administration policies
            options.AddPolicy(RoleBasedAuthorizationPolicies.CanManageSystem, policy =>
                policy.RequireRole("SuperAdmin"));

            options.AddPolicy(RoleBasedAuthorizationPolicies.CanAccessAuditLogs, policy =>
                policy.RequireRole("SuperAdmin"));

            options.AddPolicy(RoleBasedAuthorizationPolicies.CanManageSecurity, policy =>
                policy.RequireRole("SuperAdmin"));
        });

        // Register authorization handler
        services.AddScoped<IAuthorizationHandler, RoleBasedAuthorizationHandler>();

        return services;
    }
}