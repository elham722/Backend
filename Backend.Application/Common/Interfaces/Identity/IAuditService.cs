using Backend.Application.Common.DTOs.Identity;

namespace Backend.Application.Common.Interfaces.Identity
{
    /// <summary>
    /// Service interface for audit logging operations
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Logs an audit event
        /// </summary>
        Task LogAsync(AuditLogDto auditLog, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Logs a login event
        /// </summary>
        Task LogLoginAsync(string userId, bool isSuccess, string? ipAddress = null, string? userAgent = null, string? deviceInfo = null, string? sessionId = null, string? errorMessage = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Logs a logout event
        /// </summary>
        Task LogLogoutAsync(string userId, string? ipAddress = null, string? userAgent = null, string? sessionId = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Logs a role assignment event
        /// </summary>
        Task LogRoleAssignmentAsync(string userId, string roleId, string action, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Logs a permission change event
        /// </summary>
        Task LogPermissionChangeAsync(string userId, string permissionId, string action, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Logs a token operation event
        /// </summary>
        Task LogTokenOperationAsync(string userId, string action, string? tokenId = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null, bool isSuccess = true, string? errorMessage = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets audit logs with filtering
        /// </summary>
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(string? userId = null, string? action = null, string? entityType = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    }
}