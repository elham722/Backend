using Microsoft.AspNetCore.Identity;
using Backend.Identity.Models;
using Backend.Identity.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Identity.Services;

/// <summary>
/// Service for seeding initial data into the database
/// </summary>
public class DatabaseSeeder
{
    public static class DefaultRoles
    {
        public const string User = "User";
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";
    }

    public static class DefaultPermissions
    {
        // User permissions
        public const string CanViewContent = "Content:View";
        public const string CanEditProfile = "Profile:Edit";
        
        // Admin permissions
        public const string CanManageUsers = "User:Manage";
        public const string CanManageRoles = "Role:Manage";
        public const string CanViewAdminPanel = "Admin:View";
        
        // SuperAdmin permissions
        public const string CanManageSystem = "System:Manage";
        public const string CanManageAdmins = "Admin:Manage";
    }

    /// <summary>
    /// Seeds the database with initial roles, permissions, and super admin
    /// </summary>
    public static async Task SeedDatabaseAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BackendIdentityDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed roles
            await SeedRolesAsync(roleManager, logger);

            // Seed permissions
            await SeedPermissionsAsync(context, logger);

            // Seed role-permission mappings
            await SeedRolePermissionsAsync(context, logger);

            // Seed super admin
            await SeedSuperAdminAsync(userManager, configuration, logger);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager, ILogger logger)
    {
        var roles = new[]
        {
            new { Name = DefaultRoles.User, Description = "Regular user", Priority = 1, Category = "User" },
            new { Name = DefaultRoles.Admin, Description = "Administrator", Priority = 100, Category = "Admin" },
            new { Name = DefaultRoles.SuperAdmin, Description = "Super Administrator", Priority = 1000, Category = "System" }
        };

        foreach (var roleData in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleData.Name))
            {
                var role = Role.Create(
                    roleData.Name,
                    roleData.Description,
                    roleData.Category,
                    roleData.Priority,
                    roleData.Name == DefaultRoles.SuperAdmin, // SuperAdmin is system role
                    "System"
                );
                
                // Set Id for IdentityRole
                role.Id = Guid.NewGuid().ToString();

                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Role '{RoleName}' created successfully", roleData.Name);
                }
                else
                {
                    logger.LogError("Failed to create role '{RoleName}': {Errors}", 
                        roleData.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogDebug("Role '{RoleName}' already exists", roleData.Name);
            }
        }
    }

    private static async Task SeedPermissionsAsync(BackendIdentityDbContext context, ILogger logger)
    {
        var permissions = new[]
        {
            new { Name = "View Content", Resource = "Content", Action = "View", Description = "Can view content", Category = "Content", Priority = 1 },
            new { Name = "Edit Profile", Resource = "Profile", Action = "Edit", Description = "Can edit own profile", Category = "User", Priority = 2 },
            new { Name = "Manage Users", Resource = "User", Action = "Manage", Description = "Can manage users", Category = "Admin", Priority = 100 },
            new { Name = "Manage Roles", Resource = "Role", Action = "Manage", Description = "Can manage roles", Category = "Admin", Priority = 101 },
            new { Name = "View Admin Panel", Resource = "Admin", Action = "View", Description = "Can view admin panel", Category = "Admin", Priority = 102 },
            new { Name = "Manage System", Resource = "System", Action = "Manage", Description = "Can manage system", Category = "System", Priority = 1000 },
            new { Name = "Manage Admins", Resource = "Admin", Action = "Manage", Description = "Can manage administrators", Category = "System", Priority = 1001 }
        };

        foreach (var permData in permissions)
        {
            var existingPermission = await context.Permissions
                .FirstOrDefaultAsync(p => p.Resource == permData.Resource && p.Action == permData.Action);

            if (existingPermission == null)
            {
                var permission = Permission.Create(
                    permData.Name,
                    permData.Resource,
                    permData.Action,
                    permData.Description,
                    permData.Category,
                    permData.Priority,
                    permData.Priority >= 1000, // System permissions
                    "System"
                );

                context.Permissions.Add(permission);
                logger.LogInformation("Permission '{PermissionName}' created successfully", permData.Name);
            }
            else
            {
                logger.LogDebug("Permission '{PermissionName}' already exists", permData.Name);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(BackendIdentityDbContext context, ILogger logger)
    {
        // Get roles and permissions
        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == DefaultRoles.User);
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == DefaultRoles.Admin);
        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == DefaultRoles.SuperAdmin);

        var userPermissions = await context.Permissions
            .Where(p => p.Category == "User" || p.Category == "Content")
            .ToListAsync();

        var adminPermissions = await context.Permissions
            .Where(p => p.Category == "Admin")
            .ToListAsync();

        var systemPermissions = await context.Permissions
            .Where(p => p.Category == "System")
            .ToListAsync();

        // Assign permissions to User role
        if (userRole != null)
        {
            foreach (var permission in userPermissions)
            {
                var existingMapping = await context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == userRole.Id && rp.PermissionId == permission.Id);

                if (existingMapping == null)
                {
                    var rolePermission = RolePermission.Create(
                        userRole.Id,
                        permission.Id,
                        "System",
                        "Default user permissions",
                        null // No expiration
                    );

                    context.RolePermissions.Add(rolePermission);
                }
            }
        }

        // Assign permissions to Admin role
        if (adminRole != null)
        {
            var allAdminPermissions = userPermissions.Concat(adminPermissions);
            foreach (var permission in allAdminPermissions)
            {
                var existingMapping = await context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id);

                if (existingMapping == null)
                {
                    var rolePermission = RolePermission.Create(
                        adminRole.Id,
                        permission.Id,
                        "System",
                        "Default admin permissions",
                        null // No expiration
                    );

                    context.RolePermissions.Add(rolePermission);
                }
            }
        }

        // Assign all permissions to SuperAdmin role
        if (superAdminRole != null)
        {
            var allPermissions = userPermissions.Concat(adminPermissions).Concat(systemPermissions);
            foreach (var permission in allPermissions)
            {
                var existingMapping = await context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == permission.Id);

                if (existingMapping == null)
                {
                    var rolePermission = RolePermission.Create(
                        superAdminRole.Id,
                        permission.Id,
                        "System",
                        "Default super admin permissions",
                        null // No expiration
                    );

                    context.RolePermissions.Add(rolePermission);
                }
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Role-permission mappings created successfully");
    }

    private static async Task SeedSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var superAdminEmail = configuration["SuperAdmin:Email"] ?? "superadmin@site.com";
        var superAdminPassword = configuration["SuperAdmin:Password"] ?? "SuperAdmin@123";

        var existingSuperAdmin = await userManager.FindByEmailAsync(superAdminEmail);
        if (existingSuperAdmin == null)
        {
            var superAdmin = ApplicationUser.Create(
                superAdminEmail,
                "superadmin",
                null, // No customer ID initially
                "System"
            );

            var result = await userManager.CreateAsync(superAdmin, superAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, DefaultRoles.SuperAdmin);
                await userManager.SetLockoutEnabledAsync(superAdmin, false); // Never lock super admin

                logger.LogInformation("Super admin created successfully with email: {Email}", superAdminEmail);
            }
            else
            {
                logger.LogError("Failed to create super admin: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogDebug("Super admin already exists with email: {Email}", superAdminEmail);
        }
    }
}