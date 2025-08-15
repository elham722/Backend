using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Implementation of policy-based authorization service
    /// </summary>
    public class AuthorizationService : ICustomAuthorizationService
    {
        private readonly Microsoft.AspNetCore.Authorization.IAuthorizationService _aspNetAuthorizationService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            Microsoft.AspNetCore.Authorization.IAuthorizationService aspNetAuthorizationService,
            ILogger<AuthorizationService> logger)
        {
            _aspNetAuthorizationService = aspNetAuthorizationService;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission)
        {
            try
            {
                var permissions = await GetUserPermissionsAsync(user);
                return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user", permission);
                return false;
            }
        }

        public async Task<bool> HasAnyPermissionAsync(ClaimsPrincipal user, params string[] permissions)
        {
            try
            {
                var userPermissions = await GetUserPermissionsAsync(user);
                return permissions.Any(p => userPermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking any permission for user");
                return false;
            }
        }

        public async Task<bool> HasAllPermissionsAsync(ClaimsPrincipal user, params string[] permissions)
        {
            try
            {
                var userPermissions = await GetUserPermissionsAsync(user);
                return permissions.All(p => userPermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking all permissions for user");
                return false;
            }
        }

        public async Task<bool> HasRoleAsync(ClaimsPrincipal user, string role)
        {
            try
            {
                var roles = await GetUserRolesAsync(user);
                return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role {Role} for user", role);
                return false;
            }
        }

        public async Task<bool> HasAnyRoleAsync(ClaimsPrincipal user, params string[] roles)
        {
            try
            {
                var userRoles = await GetUserRolesAsync(user);
                return roles.Any(r => userRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking any role for user");
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(ClaimsPrincipal user)
        {
            try
            {
                var permissions = new List<string>();

                // Get permissions from claims
                var permissionClaims = user.FindAll("permission");
                permissions.AddRange(permissionClaims.Select(c => c.Value));

                // Get permissions from roles (you can implement role-to-permission mapping here)
                var roles = await GetUserRolesAsync(user);
                foreach (var role in roles)
                {
                    var rolePermissions = GetPermissionsForRole(role);
                    permissions.AddRange(rolePermissions);
                }

                return permissions.Distinct(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for user");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(ClaimsPrincipal user)
        {
            try
            {
                var roleClaims = user.FindAll(ClaimTypes.Role);
                return roleClaims.Select(c => c.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<bool> ValidatePolicyAsync(ClaimsPrincipal user, string policyName, object? resource = null)
        {
            try
            {
                var requirement = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                var result = await _aspNetAuthorizationService.AuthorizeAsync(user, resource, requirement);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating policy {PolicyName} for user", policyName);
                return false;
            }
        }

        private static IEnumerable<string> GetPermissionsForRole(string role)
        {
            // This is a simple role-to-permission mapping
            // In a real application, you might want to store this in a database
            return role.ToLowerInvariant() switch
            {
                "admin" => new[]
                {
                    "users.read", "users.write", "users.delete",
                    "customers.read", "customers.write", "customers.delete",
                    "orders.read", "orders.write", "orders.delete",
                    "reports.read", "reports.write"
                },
                "manager" => new[]
                {
                    "users.read", "customers.read", "customers.write",
                    "orders.read", "orders.write", "reports.read"
                },
                "user" => new[]
                {
                    "customers.read", "orders.read", "orders.write"
                },
                _ => Enumerable.Empty<string>()
            };
        }
    }
} 