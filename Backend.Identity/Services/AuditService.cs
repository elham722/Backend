using Backend.Identity.Models;
using Backend.Identity.Context;
using Microsoft.EntityFrameworkCore;
using Backend.Application.Common.Interfaces.Identity;
using Backend.Application.Common.DTOs.Identity;

namespace Backend.Identity.Services
{
    public class AuditService : IAuditService
    {
        private readonly BackendIdentityDbContext _context;

        public AuditService(BackendIdentityDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task LogAsync(AuditLogDto auditLogDto, CancellationToken cancellationToken = default)
        {
            var auditLog = AuditLog.Create(
                auditLogDto.UserId,
                auditLogDto.Action,
                auditLogDto.EntityType,
                auditLogDto.EntityId,
                auditLogDto.OldValues,
                auditLogDto.NewValues,
                auditLogDto.IpAddress,
                auditLogDto.UserAgent,
                auditLogDto.DeviceInfo,
                auditLogDto.SessionId,
                auditLogDto.RequestId,
                auditLogDto.AdditionalData,
                auditLogDto.IsSuccess,
                auditLogDto.ErrorMessage,
                auditLogDto.Severity
            );
            
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task LogLoginAsync(string userId, bool isSuccess, string? ipAddress = null, string? userAgent = null, string? deviceInfo = null, string? sessionId = null, string? errorMessage = null, CancellationToken cancellationToken = default)
        {
            var auditLog = AuditLog.CreateForLogin(userId, isSuccess, ipAddress, userAgent, deviceInfo, sessionId, errorMessage);
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task LogLogoutAsync(string userId, string? ipAddress = null, string? userAgent = null, string? sessionId = null, CancellationToken cancellationToken = default)
        {
            var auditLog = AuditLog.CreateForLogout(userId, ipAddress, userAgent, sessionId);
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task LogRoleAssignmentAsync(string userId, string roleId, string action, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null, CancellationToken cancellationToken = default)
        {
            var auditLog = AuditLog.CreateForRoleAssignment(userId, roleId, action, oldValues, newValues, ipAddress, userAgent, additionalData);
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task LogPermissionChangeAsync(string userId, string permissionId, string action, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null, CancellationToken cancellationToken = default)
        {
            var auditLog = AuditLog.CreateForPermissionChange(userId, permissionId, action, oldValues, newValues, ipAddress, userAgent, additionalData);
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task LogTokenOperationAsync(string userId, string action, string? tokenId = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null, bool isSuccess = true, string? errorMessage = null, CancellationToken cancellationToken = default)
        {
            var auditLog = AuditLog.CreateForTokenOperation(userId, action, tokenId, ipAddress, userAgent, additionalData, isSuccess, errorMessage);
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(string? userId = null, string? action = null, string? entityType = null, DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(al => al.UserId == userId);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(al => al.Action == action);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(al => al.EntityType == entityType);

            if (fromDate.HasValue)
                query = query.Where(al => al.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(al => al.Timestamp <= toDate.Value);

            var auditLogs = await query
                .OrderByDescending(al => al.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(al => new AuditLogDto
                {
                    Id = al.Id,
                    UserId = al.UserId,
                    Action = al.Action,
                    EntityType = al.EntityType,
                    EntityId = al.EntityId,
                    OldValues = al.OldValues,
                    NewValues = al.NewValues,
                    Timestamp = al.Timestamp,
                    IpAddress = al.IpAddress,
                    UserAgent = al.UserAgent,
                    DeviceInfo = al.DeviceInfo,
                    SessionId = al.SessionId,
                    RequestId = al.RequestId,
                    AdditionalData = al.AdditionalData,
                    IsSuccess = al.IsSuccess,
                    ErrorMessage = al.ErrorMessage,
                    Severity = al.Severity
                })
                .ToListAsync(cancellationToken);

            return auditLogs;
        }
    }
}