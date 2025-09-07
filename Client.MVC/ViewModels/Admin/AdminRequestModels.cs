namespace Client.MVC.ViewModels.Admin
{
    /// <summary>
    /// Request model for creating a new role
    /// </summary>
    public class CreateRoleRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Category { get; set; }
        public int Priority { get; set; } = 0;
        public bool IsSystemRole { get; set; } = false;
    }

    /// <summary>
    /// Request model for updating a role
    /// </summary>
    public class UpdateRoleRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Category { get; set; }
        public int Priority { get; set; } = 0;
        public bool IsSystemRole { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request model for creating a new permission
    /// </summary>
    public class CreatePermissionRequest
    {
        public string Name { get; set; } = null!;
        public string Resource { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Category { get; set; }
        public int Priority { get; set; } = 0;
        public bool IsSystemPermission { get; set; } = false;
    }

    /// <summary>
    /// Request model for updating a permission
    /// </summary>
    public class UpdatePermissionRequest
    {
        public string Name { get; set; } = null!;
        public string Resource { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Category { get; set; }
        public int Priority { get; set; } = 0;
        public bool IsSystemPermission { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request model for assigning permission to role
    /// </summary>
    public class AssignPermissionRequest
    {
        public DateTime? ExpiresAt { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request model for assigning role to user
    /// </summary>
    public class AssignRoleRequest
    {
        public string UserId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request model for removing role from user
    /// </summary>
    public class RemoveRoleRequest
    {
        public string UserId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request model for checking permission
    /// </summary>
    public class CheckPermissionRequest
    {
        public string Resource { get; set; } = null!;
        public string Action { get; set; } = null!;
    }
}