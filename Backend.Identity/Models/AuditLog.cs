using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Models
{
    public class AuditLog
    {
        public string Id { get; private set; } = null!;
        public string UserId { get; private set; } = null!;
        public string Action { get; private set; } = null!;
        public string EntityType { get; private set; } = null!;
        public string EntityId { get; private set; } = null!;
        public string? OldValues { get; private set; }
        public string? NewValues { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string IpAddress { get; private set; } = null!;
        public string? UserAgent { get; private set; }
        public string? DeviceInfo { get; private set; }
        public string? SessionId { get; private set; }
        public string? RequestId { get; private set; }
        public string? AdditionalData { get; private set; }
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? Severity { get; private set; } = "Info";

        private AuditLog() { } // For EF Core

        public static AuditLog Create(string userId, string action, string entityType, string entityId, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? deviceInfo = null, string? sessionId = null, string? requestId = null, string? additionalData = null, bool isSuccess = true, string? errorMessage = null, string severity = "Info")
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be null or empty", nameof(action));

            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("Entity type cannot be null or empty", nameof(entityType));

            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

            return new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress ?? "Unknown",
                UserAgent = userAgent,
                DeviceInfo = deviceInfo,
                SessionId = sessionId,
                RequestId = requestId,
                AdditionalData = additionalData,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                Severity = severity
            };
        }

        public static AuditLog CreateForLogin(string userId, bool isSuccess, string? ipAddress = null, string? userAgent = null, string? deviceInfo = null, string? sessionId = null, string? errorMessage = null)
        {
            return Create(
                userId: userId,
                action: isSuccess ? "Login" : "LoginFailed",
                entityType: "User",
                entityId: userId,
                ipAddress: ipAddress,
                userAgent: userAgent,
                deviceInfo: deviceInfo,
                sessionId: sessionId,
                isSuccess: isSuccess,
                errorMessage: errorMessage,
                severity: isSuccess ? "Info" : "Warning"
            );
        }

        public static AuditLog CreateForLogout(string userId, string? ipAddress = null, string? userAgent = null, string? sessionId = null)
        {
            return Create(
                userId: userId,
                action: "Logout",
                entityType: "User",
                entityId: userId,
                ipAddress: ipAddress,
                userAgent: userAgent,
                sessionId: sessionId,
                isSuccess: true,
                severity: "Info"
            );
        }

        public static AuditLog CreateForRoleAssignment(string userId, string roleId, string action, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null)
        {
            return Create(
                userId: userId,
                action: action,
                entityType: "UserRole",
                entityId: roleId,
                oldValues: oldValues,
                newValues: newValues,
                ipAddress: ipAddress,
                userAgent: userAgent,
                additionalData: additionalData,
                isSuccess: true,
                severity: "Info"
            );
        }

        public static AuditLog CreateForPermissionChange(string userId, string permissionId, string action, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null)
        {
            return Create(
                userId: userId,
                action: action,
                entityType: "Permission",
                entityId: permissionId,
                oldValues: oldValues,
                newValues: newValues,
                ipAddress: ipAddress,
                userAgent: userAgent,
                additionalData: additionalData,
                isSuccess: true,
                severity: "Info"
            );
        }

        public static AuditLog CreateForTokenOperation(string userId, string action, string? tokenId = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null, bool isSuccess = true, string? errorMessage = null)
        {
            return Create(
                userId: userId,
                action: action,
                entityType: "Token",
                entityId: tokenId ?? "Unknown",
                ipAddress: ipAddress,
                userAgent: userAgent,
                additionalData: additionalData,
                isSuccess: isSuccess,
                errorMessage: errorMessage,
                severity: isSuccess ? "Info" : "Warning"
            );
        }

        public void UpdateSuccess(bool isSuccess, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Severity = isSuccess ? "Info" : "Warning";
        }

        public void AddAdditionalData(string additionalData)
        {
            AdditionalData = string.IsNullOrEmpty(AdditionalData) 
                ? additionalData 
                : $"{AdditionalData}; {additionalData}";
        }
    }
}