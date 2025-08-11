using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Models
{
    public class UserToken : IdentityUserToken<string>
    {
        public string UserId { get; set; } = null!;
        public string LoginProvider { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Value { get; set; }
        
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = null!;
        public string? UpdatedBy { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public string? DeviceInfo { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedBy { get; private set; }
        public string? RevocationReason { get; private set; }

        private UserToken() { } // For EF Core

        public static UserToken Create(string userId, string loginProvider, string name, string value, DateTime? expiresAt = null, string? deviceInfo = null, string? ipAddress = null, string? userAgent = null, string createdBy = "System")
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(loginProvider))
                throw new ArgumentException("Login provider cannot be null or empty", nameof(loginProvider));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Token name cannot be null or empty", nameof(name));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Token value cannot be null or empty", nameof(value));

            return new UserToken
            {
                UserId = userId,
                LoginProvider = loginProvider,
                Name = name,
                Value = value,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActive = true,
                ExpiresAt = expiresAt,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsRevoked = false
            };
        }

        public void UpdateValue(string newValue, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(newValue))
                throw new ArgumentException("Token value cannot be null or empty", nameof(newValue));

            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("Updated by cannot be null or empty", nameof(updatedBy));

            if (IsRevoked)
                throw new InvalidOperationException("Cannot update a revoked token");

            Value = newValue;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void UpdateDeviceInfo(string? deviceInfo, string? ipAddress, string? userAgent, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("Updated by cannot be null or empty", nameof(updatedBy));

            if (IsRevoked)
                throw new InvalidOperationException("Cannot update a revoked token");

            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void ExtendExpiration(DateTime newExpiresAt, string extendedBy)
        {
            if (string.IsNullOrWhiteSpace(extendedBy))
                throw new ArgumentException("Extended by cannot be null or empty", nameof(extendedBy));

            if (newExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException("New expiration date must be in the future", nameof(newExpiresAt));

            if (IsRevoked)
                throw new InvalidOperationException("Cannot extend expiration of a revoked token");

            ExpiresAt = newExpiresAt;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = extendedBy;
        }

        public void Revoke(string revokedBy, string? revocationReason = null)
        {
            if (string.IsNullOrWhiteSpace(revokedBy))
                throw new ArgumentException("Revoked by cannot be null or empty", nameof(revokedBy));

            if (IsRevoked)
                throw new InvalidOperationException("Token is already revoked");

            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            RevokedBy = revokedBy;
            RevocationReason = revocationReason;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = revokedBy;
        }

        public void Deactivate(string deactivatedBy)
        {
            if (string.IsNullOrWhiteSpace(deactivatedBy))
                throw new ArgumentException("Deactivated by cannot be null or empty", nameof(deactivatedBy));

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = deactivatedBy;
        }

        public void Activate(string activatedBy)
        {
            if (string.IsNullOrWhiteSpace(activatedBy))
                throw new ArgumentException("Activated by cannot be null or empty", nameof(activatedBy));

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = activatedBy;
        }

        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        }

        public bool IsValid()
        {
            return IsActive && !IsRevoked && !IsExpired();
        }

        public bool IsFromSameDevice(string? deviceInfo, string? ipAddress, string? userAgent)
        {
            return DeviceInfo == deviceInfo && IpAddress == ipAddress && UserAgent == userAgent;
        }

        public TimeSpan? GetRemainingTime()
        {
            if (!ExpiresAt.HasValue || IsExpired())
                return null;

            return ExpiresAt.Value - DateTime.UtcNow;
        }
    }
} 