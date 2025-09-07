using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Common.Authorization;

namespace Backend.Application.Common.Authorization
{
    public static class AuthorizationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
        {
            // Add authorization handlers
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, ResourceOwnershipAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, BranchAccessAuthorizationHandler>();

            // Configure authorization policies
            services.AddAuthorization(options =>
            {
                // Permission-based policies
                options.AddPolicy(AuthorizationPolicies.CanReadUsers, policy =>
                    policy.Requirements.Add(new PermissionRequirement("User", "Read")));

                options.AddPolicy(AuthorizationPolicies.CanCreateUsers, policy =>
                    policy.Requirements.Add(new PermissionRequirement("User", "Create")));

                options.AddPolicy(AuthorizationPolicies.CanUpdateUsers, policy =>
                    policy.Requirements.Add(new PermissionRequirement("User", "Update")));

                options.AddPolicy(AuthorizationPolicies.CanDeleteUsers, policy =>
                    policy.Requirements.Add(new PermissionRequirement("User", "Delete")));

                options.AddPolicy(AuthorizationPolicies.CanReadRoles, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role", "Read")));

                options.AddPolicy(AuthorizationPolicies.CanCreateRoles, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role", "Create")));

                options.AddPolicy(AuthorizationPolicies.CanUpdateRoles, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role", "Update")));

                options.AddPolicy(AuthorizationPolicies.CanDeleteRoles, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role", "Delete")));

                options.AddPolicy(AuthorizationPolicies.CanReadPermissions, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Permission", "Read")));

                options.AddPolicy(AuthorizationPolicies.CanCreatePermissions, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Permission", "Create")));

                options.AddPolicy(AuthorizationPolicies.CanUpdatePermissions, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Permission", "Update")));

                options.AddPolicy(AuthorizationPolicies.CanDeletePermissions, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Permission", "Delete")));

                options.AddPolicy(AuthorizationPolicies.CanAssignRoles, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role", "Assign")));

                options.AddPolicy(AuthorizationPolicies.CanRevokeRoles, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role", "Revoke")));

                options.AddPolicy(AuthorizationPolicies.CanManageRolePermissions, policy =>
                    policy.Requirements.Add(new PermissionRequirement("RolePermission", "Manage")));

                // Resource-based policies
                options.AddPolicy(AuthorizationPolicies.CanApproveTransaction, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Transaction", "Approve")));

                options.AddPolicy(AuthorizationPolicies.CanViewOwnTransactions, policy =>
                    policy.Requirements.Add(new ResourceOwnershipRequirement("Transaction")));

                options.AddPolicy(AuthorizationPolicies.CanViewBranchTransactions, policy =>
                    policy.Requirements.Add(new BranchAccessRequirement()));

                options.AddPolicy(AuthorizationPolicies.CanManageOwnProfile, policy =>
                    policy.Requirements.Add(new ResourceOwnershipRequirement("User")));

                // Admin policies
                options.AddPolicy(AuthorizationPolicies.IsAdmin, policy =>
                    policy.Requirements.Add(new RoleRequirement("Admin")));

                options.AddPolicy(AuthorizationPolicies.IsSuperAdmin, policy =>
                    policy.Requirements.Add(new RoleRequirement("SuperAdmin")));

                options.AddPolicy(AuthorizationPolicies.CanAccessAdminPanel, policy =>
                    policy.Requirements.Add(new PermissionRequirement("Admin", "Access")));

                // System policies
                options.AddPolicy(AuthorizationPolicies.CanViewAuditLogs, policy =>
                    policy.Requirements.Add(new PermissionRequirement("AuditLog", "Read")));

                options.AddPolicy(AuthorizationPolicies.CanManageSystemSettings, policy =>
                    policy.Requirements.Add(new PermissionRequirement("System", "Manage")));
            });

            return services;
        }
    }
}