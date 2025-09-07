using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.Common.Authorization;
using Backend.Application.Common.Interfaces.Identity;
using Backend.Application.Common.DTOs.Identity;
using Backend.Application.Common.Results;

namespace Backend.Api.Controllers.V1.Auth
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class AuthorizationController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuditService _auditService;

        public AuthorizationController(IUserService userService, IAuditService auditService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <summary>
        /// Get current user's permissions
        /// </summary>
        [HttpGet("permissions")]
        [Authorize(Policy = AuthorizationPolicies.CanReadPermissions)]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetUserPermissions(CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var permissions = await _userService.GetUserPermissionsAsync(userId, cancellationToken);
            return Ok(permissions);
        }

        /// <summary>
        /// Get current user's roles
        /// </summary>
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var roles = await _userService.GetUserRolesAsync(userId, cancellationToken);
            return Ok(roles);
        }

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        [HttpPost("check-permission")]
        public async Task<ActionResult<bool>> CheckPermission([FromBody] CheckPermissionRequest request, CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var hasPermission = await _userService.HasPermissionAsync(userId, request.Resource, request.Action, cancellationToken);
            return Ok(hasPermission);
        }

        /// <summary>
        /// Assign role to user (Admin only)
        /// </summary>
        [HttpPost("assign-role")]
        [Authorize(Policy = AuthorizationPolicies.CanAssignRoles)]
        public async Task<ActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken = default)
        {
            var assignedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(assignedBy))
                return Unauthorized();

            var success = await _userService.AssignRoleAsync(request.UserId, request.RoleName, assignedBy, request.ExpiresAt, request.Reason, cancellationToken);
            
            if (success)
            {
                await _auditService.LogRoleAssignmentAsync(assignedBy, request.RoleName, "Assign", 
                    null, $"Role: {request.RoleName}", Request.HttpContext.Connection.RemoteIpAddress?.ToString(), 
                    Request.Headers.UserAgent.ToString());
                
                return Ok();
            }

            return BadRequest("Failed to assign role");
        }

        /// <summary>
        /// Remove role from user (Admin only)
        /// </summary>
        [HttpPost("remove-role")]
        [Authorize(Policy = AuthorizationPolicies.CanRevokeRoles)]
        public async Task<ActionResult> RemoveRole([FromBody] RemoveRoleRequest request, CancellationToken cancellationToken = default)
        {
            var removedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(removedBy))
                return Unauthorized();

            var success = await _userService.RemoveRoleAsync(request.UserId, request.RoleName, removedBy, request.Reason, cancellationToken);
            
            if (success)
            {
                await _auditService.LogRoleAssignmentAsync(removedBy, request.RoleName, "Remove", 
                    $"Role: {request.RoleName}", null, Request.HttpContext.Connection.RemoteIpAddress?.ToString(), 
                    Request.Headers.UserAgent.ToString());
                
                return Ok();
            }

            return BadRequest("Failed to remove role");
        }

        /// <summary>
        /// Get all roles (Admin only)
        /// </summary>
        [HttpGet("roles/all")]
        [Authorize(Policy = AuthorizationPolicies.CanReadRoles)]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles(CancellationToken cancellationToken = default)
        {
            var roles = await _userService.GetAllRolesAsync(false, cancellationToken);
            return Ok(roles);
        }

        /// <summary>
        /// Get all permissions (Admin only)
        /// </summary>
        [HttpGet("permissions/all")]
        [Authorize(Policy = AuthorizationPolicies.CanReadPermissions)]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAllPermissions(CancellationToken cancellationToken = default)
        {
            var permissions = await _userService.GetAllPermissionsAsync(false, cancellationToken);
            return Ok(permissions);
        }

        /// <summary>
        /// Create new role (Admin only)
        /// </summary>
        [HttpPost("roles")]
        [Authorize(Policy = AuthorizationPolicies.CanCreateRoles)]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken = default)
        {
            var createdBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(createdBy))
                return Unauthorized();

            try
            {
                var role = await _userService.CreateRoleAsync(request.Name, request.Description, request.Category, 
                    request.Priority, request.IsSystemRole, createdBy, cancellationToken);
                
                await _auditService.LogRoleAssignmentAsync(createdBy, role.Id, "Create", 
                    null, $"Role: {role.Name}", Request.HttpContext.Connection.RemoteIpAddress?.ToString(), 
                    Request.Headers.UserAgent.ToString());
                
                return Ok(role);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Create new permission (Admin only)
        /// </summary>
        [HttpPost("permissions")]
        [Authorize(Policy = AuthorizationPolicies.CanCreatePermissions)]
        public async Task<ActionResult<PermissionDto>> CreatePermission([FromBody] CreatePermissionRequest request, CancellationToken cancellationToken = default)
        {
            var createdBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(createdBy))
                return Unauthorized();

            try
            {
                var permission = await _userService.CreatePermissionAsync(request.Name, request.Resource, request.Action, 
                    request.Description, request.Category, request.Priority, request.IsSystemPermission, createdBy, cancellationToken);
                
                await _auditService.LogPermissionChangeAsync(createdBy, permission.Id, "Create", 
                    null, $"Permission: {permission.Name}", Request.HttpContext.Connection.RemoteIpAddress?.ToString(), 
                    Request.Headers.UserAgent.ToString());
                
                return Ok(permission);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Assign permission to role (Admin only)
        /// </summary>
        [HttpPost("roles/{roleId}/permissions/{permissionId}")]
        [Authorize(Policy = AuthorizationPolicies.CanManageRolePermissions)]
        public async Task<ActionResult> AssignPermissionToRole(string roleId, string permissionId, [FromBody] AssignPermissionRequest request, CancellationToken cancellationToken = default)
        {
            var assignedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(assignedBy))
                return Unauthorized();

            var success = await _userService.AssignPermissionToRoleAsync(roleId, permissionId, assignedBy, request.ExpiresAt, request.Reason, cancellationToken);
            
            if (success)
            {
                await _auditService.LogPermissionChangeAsync(assignedBy, permissionId, "AssignToRole", 
                    null, $"Role: {roleId}, Permission: {permissionId}", Request.HttpContext.Connection.RemoteIpAddress?.ToString(), 
                    Request.Headers.UserAgent.ToString());
                
                return Ok();
            }

            return BadRequest("Failed to assign permission to role");
        }

        /// <summary>
        /// Remove permission from role (Admin only)
        /// </summary>
        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        [Authorize(Policy = AuthorizationPolicies.CanManageRolePermissions)]
        public async Task<ActionResult> RemovePermissionFromRole(string roleId, string permissionId, [FromBody] RemovePermissionRequest request, CancellationToken cancellationToken = default)
        {
            var removedBy = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(removedBy))
                return Unauthorized();

            var success = await _userService.RemovePermissionFromRoleAsync(roleId, permissionId, removedBy, request.Reason, cancellationToken);
            
            if (success)
            {
                await _auditService.LogPermissionChangeAsync(removedBy, permissionId, "RemoveFromRole", 
                    $"Role: {roleId}, Permission: {permissionId}", null, Request.HttpContext.Connection.RemoteIpAddress?.ToString(), 
                    Request.Headers.UserAgent.ToString());
                
                return Ok();
            }

            return BadRequest("Failed to remove permission from role");
        }
    }

    // Request DTOs
    public class CheckPermissionRequest
    {
        public string Resource { get; set; } = null!;
        public string Action { get; set; } = null!;
    }

    public class AssignRoleRequest
    {
        public string UserId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public string? Reason { get; set; }
    }

    public class RemoveRoleRequest
    {
        public string UserId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string? Reason { get; set; }
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Category { get; set; }
        public int Priority { get; set; }
        public bool IsSystemRole { get; set; }
    }

    public class CreatePermissionRequest
    {
        public string Name { get; set; } = null!;
        public string Resource { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Category { get; set; }
        public int Priority { get; set; }
        public bool IsSystemPermission { get; set; }
    }

    public class AssignPermissionRequest
    {
        public DateTime? ExpiresAt { get; set; }
        public string? Reason { get; set; }
    }

    public class RemovePermissionRequest
    {
        public string? Reason { get; set; }
    }
}