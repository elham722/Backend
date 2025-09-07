namespace Backend.Application.Common.DTOs.Identity
{
    /// <summary>
    /// DTO for Role information
    /// </summary>
    public class RoleDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemRole { get; set; }
        public int Priority { get; set; }
        public string? Category { get; set; }
    }

    /// <summary>
    /// DTO for Permission information
    /// </summary>
    public class PermissionDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Resource { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemPermission { get; set; }
        public string? Category { get; set; }
        public int Priority { get; set; }
        
        /// <summary>
        /// Gets the full permission name in Resource:Action format
        /// </summary>
        public string FullName => $"{Resource}:{Action}";
    }

    /// <summary>
    /// DTO for RolePermission information
    /// </summary>
    public class RolePermissionDto
    {
        public string Id { get; set; } = null!;
        public string RoleId { get; set; } = null!;
        public string PermissionId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? AssignedBy { get; set; }
        public string? AssignmentReason { get; set; }
        public bool IsGranted { get; set; }
        
        // Navigation properties
        public RoleDto Role { get; set; } = null!;
        public PermissionDto Permission { get; set; } = null!;
    }

    /// <summary>
    /// DTO for AuditLog information
    /// </summary>
    public class AuditLogDto
    {
        public string Id { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string EntityType { get; set; } = null!;
        public string EntityId { get; set; } = null!;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = null!;
        public string? UserAgent { get; set; }
        public string? DeviceInfo { get; set; }
        public string? SessionId { get; set; }
        public string? RequestId { get; set; }
        public string? AdditionalData { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Severity { get; set; }
    }
}