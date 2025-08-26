using System;
using System.Collections.Generic;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Common;
using Backend.Domain.Events.Identity;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities
{
    public class RefreshToken : BaseAggregateRoot<Guid>
    {
        // Basic Information
        public string TokenHash { get; private set; } = null!;
        public string UserId { get; private set; } = null!;
        public DateTime ExpiresAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedBy { get; private set; }
        public string? RevocationReason { get; private set; }
        public Guid? ReplacedById { get; private set; }
        public RefreshToken? ReplacedBy { get; private set; }
        public RefreshToken? Replaces { get; private set; }

        // Device Information
        public string? DeviceInfo { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public string? DeviceId { get; private set; }
        public string? DeviceType { get; private set; }
        public string? DeviceName { get; private set; }
        public string? OperatingSystem { get; private set; }
        public string? Browser { get; private set; }

        // Security Information
        public bool IsRevoked { get; private set; }
        public bool IsRotated { get; private set; }
        public int RotationCount { get; private set; }
        public DateTime? LastUsedAt { get; private set; }
        public int UsageCount { get; private set; }

        // Computed Properties
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsRevoked && !IsExpired && !IsRotated;
        public bool IsActive => IsValid && !IsRotated;
        public TimeSpan RemainingTime => IsExpired ? TimeSpan.Zero : ExpiresAt - DateTime.UtcNow;
        public bool HasDeviceInfo => !string.IsNullOrWhiteSpace(DeviceInfo) || !string.IsNullOrWhiteSpace(DeviceId);

        private RefreshToken() { } // For EF Core

        private RefreshToken(Guid id, string tokenHash, string userId, DateTime expiresAt, string? createdBy = null)
        {
            ValidateId(id);
            Guard.AgainstNullOrEmpty(tokenHash, nameof(tokenHash));
            Guard.AgainstNullOrEmpty(userId, nameof(userId));

            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

            Id = id;
            TokenHash = tokenHash;
            UserId = userId;
            ExpiresAt = expiresAt;
            IsRevoked = false;
            IsRotated = false;
            RotationCount = 0;
            UsageCount = 0;

            if (!string.IsNullOrWhiteSpace(createdBy))
                SetCreatedBy(createdBy);
        }

        public static RefreshToken Create(string tokenHash, string userId, DateTime expiresAt, string? createdBy = null)
        {
            var refreshToken = new RefreshToken(Guid.NewGuid(), tokenHash, userId, expiresAt, createdBy);
            refreshToken.AddDomainEvent(new RefreshTokenCreatedEvent(refreshToken.Id, refreshToken.UserId));
            refreshToken.OnCreated();
            return refreshToken;
        }

        public static RefreshToken Create(string tokenHash, string userId, TimeSpan validityPeriod, string? createdBy = null)
        {
            var expiresAt = DateTime.UtcNow.Add(validityPeriod);
            return Create(tokenHash, userId, expiresAt, createdBy);
        }

        public static RefreshToken CreateWithDeviceInfo(string tokenHash, string userId, DateTime expiresAt, 
            string? deviceInfo = null, string? ipAddress = null, string? userAgent = null, string? createdBy = null)
        {
            var refreshToken = Create(tokenHash, userId, expiresAt, createdBy);
            refreshToken.SetDeviceInfo(deviceInfo, ipAddress, userAgent);
            return refreshToken;
        }

        // Device Information Methods
        public void SetDeviceInfo(string? deviceInfo, string? ipAddress, string? userAgent)
        {
            DeviceInfo = deviceInfo?.Trim();
            IpAddress = ipAddress?.Trim();
            UserAgent = userAgent?.Trim();
            
            ExtractDeviceDetails();
            OnUpdated();
        }

        public void SetDeviceInfo(string? deviceInfo, string? ipAddress, string? userAgent, 
            string? deviceId, string? deviceType, string? deviceName, string? operatingSystem, string? browser)
        {
            DeviceInfo = deviceInfo?.Trim();
            IpAddress = ipAddress?.Trim();
            UserAgent = userAgent?.Trim();
            DeviceId = deviceId?.Trim();
            DeviceType = deviceType?.Trim();
            DeviceName = deviceName?.Trim();
            OperatingSystem = operatingSystem?.Trim();
            Browser = browser?.Trim();
            
            OnUpdated();
        }

        public void UpdateDeviceInfo(string? deviceInfo, string? ipAddress, string? userAgent, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            SetDeviceInfo(deviceInfo, ipAddress, userAgent);
            SetUpdatedBy(updatedBy);
        }

        // Token Management Methods
        public void ExtendExpiration(DateTime newExpiresAt, string extendedBy)
        {
            Guard.AgainstNullOrEmpty(extendedBy, nameof(extendedBy));

            if (newExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException("New expiration date must be in the future", nameof(newExpiresAt));

            if (IsRevoked)
                throw new InvalidOperationException("Cannot extend expiration of a revoked token");

            if (IsRotated)
                throw new InvalidOperationException("Cannot extend expiration of a rotated token");

            var oldExpiresAt = ExpiresAt;
            ExpiresAt = newExpiresAt;
            SetUpdatedBy(extendedBy);
            OnUpdated();
            
            AddDomainEvent(new RefreshTokenExtendedEvent(Id, UserId, oldExpiresAt, ExpiresAt));
        }

        public void Revoke(string revokedBy, string? revocationReason = null)
        {
            Guard.AgainstNullOrEmpty(revokedBy, nameof(revokedBy));

            if (IsRevoked)
                throw new InvalidOperationException("Token is already revoked");

            if (IsRotated)
                throw new InvalidOperationException("Cannot revoke a rotated token");

            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            RevokedBy = revokedBy;
            RevocationReason = revocationReason?.Trim();
            SetUpdatedBy(revokedBy);
            OnUpdated();
            
            AddDomainEvent(new RefreshTokenRevokedEvent(Id, UserId, RevocationReason));
        }

        public void Rotate(string newTokenHash, RefreshToken newToken, string rotatedBy)
        {
            Guard.AgainstNullOrEmpty(newTokenHash, nameof(newTokenHash));
            Guard.AgainstNull(newToken, nameof(newToken));
            Guard.AgainstNullOrEmpty(rotatedBy, nameof(rotatedBy));

            if (IsRevoked)
                throw new InvalidOperationException("Cannot rotate a revoked token");

            if (IsRotated)
                throw new InvalidOperationException("Token is already rotated");

            if (newToken.UserId != UserId)
                throw new InvalidOperationException("New token must belong to the same user");

            IsRotated = true;
            ReplacedById = newToken.Id;
            ReplacedBy = newToken;
            newToken.Replaces = this;
            RotationCount++;
            SetUpdatedBy(rotatedBy);
            OnUpdated();
            
            AddDomainEvent(new RefreshTokenRotatedEvent(Id, UserId, newToken.Id));
        }

        public void MarkAsUsed(string usedBy)
        {
            Guard.AgainstNullOrEmpty(usedBy, nameof(usedBy));

            if (IsRevoked || IsRotated || IsExpired)
                throw new InvalidOperationException("Cannot use an invalid token");

            LastUsedAt = DateTime.UtcNow;
            UsageCount++;
            SetUpdatedBy(usedBy);
            OnUpdated();
        }

        // Business Logic Methods
        public bool CanBeUsed()
        {
            return IsValid && !IsRotated;
        }

        public bool CanBeRotated()
        {
            return IsValid && !IsRotated;
        }

        public bool CanBeExtended()
        {
            return IsValid && !IsRotated;
        }

        public bool CanBeRevoked()
        {
            return !IsRevoked && !IsRotated;
        }

        public bool IsFromSameDevice(string? deviceInfo, string? ipAddress, string? userAgent)
        {
            if (!HasDeviceInfo)
                return false;

            var deviceMatch = string.IsNullOrWhiteSpace(DeviceInfo) || 
                             string.IsNullOrWhiteSpace(deviceInfo) || 
                             DeviceInfo.Equals(deviceInfo, StringComparison.OrdinalIgnoreCase);

            var ipMatch = string.IsNullOrWhiteSpace(IpAddress) || 
                         string.IsNullOrWhiteSpace(ipAddress) || 
                         IpAddress.Equals(ipAddress, StringComparison.OrdinalIgnoreCase);

            var userAgentMatch = string.IsNullOrWhiteSpace(UserAgent) || 
                                string.IsNullOrWhiteSpace(userAgent) || 
                                UserAgent.Equals(userAgent, StringComparison.OrdinalIgnoreCase);

            return deviceMatch && ipMatch && userAgentMatch;
        }

        public bool IsExpiringSoon(int minutesThreshold = 30)
        {
            if (IsExpired || IsRevoked || IsRotated)
                return false;

            var timeUntilExpiry = ExpiresAt - DateTime.UtcNow;
            return timeUntilExpiry.TotalMinutes <= minutesThreshold;
        }

        public string GetStatusDescription()
        {
            if (IsRevoked)
                return "Revoked";

            if (IsRotated)
                return "Rotated";

            if (IsExpired)
                return "Expired";

            if (IsExpiringSoon())
                return "Expiring Soon";

            return "Active";
        }

        public string GetDeviceDescription()
        {
            if (!HasDeviceInfo)
                return "Unknown Device";

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(DeviceType))
                parts.Add(DeviceType);

            if (!string.IsNullOrWhiteSpace(OperatingSystem))
                parts.Add(OperatingSystem);

            if (!string.IsNullOrWhiteSpace(Browser))
                parts.Add(Browser);

            if (!string.IsNullOrWhiteSpace(DeviceName))
                parts.Add(DeviceName);

            if (parts.Count == 0)
                return DeviceInfo ?? "Unknown Device";

            return string.Join(" - ", parts);
        }

        // Helper Methods
        private void ExtractDeviceDetails()
        {
            if (string.IsNullOrWhiteSpace(UserAgent))
                return;

            // Basic device type detection
            var userAgent = UserAgent.ToLowerInvariant();
            
            if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
                DeviceType = "Mobile";
            else if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
                DeviceType = "Tablet";
            else
                DeviceType = "Desktop";

            // Operating system detection
            if (userAgent.Contains("windows"))
                OperatingSystem = "Windows";
            else if (userAgent.Contains("mac os") || userAgent.Contains("macintosh"))
                OperatingSystem = "macOS";
            else if (userAgent.Contains("linux"))
                OperatingSystem = "Linux";
            else if (userAgent.Contains("android"))
                OperatingSystem = "Android";
            else if (userAgent.Contains("ios"))
                OperatingSystem = "iOS";

            // Browser detection
            if (userAgent.Contains("chrome"))
                Browser = "Chrome";
            else if (userAgent.Contains("firefox"))
                Browser = "Firefox";
            else if (userAgent.Contains("safari"))
                Browser = "Safari";
            else if (userAgent.Contains("edge"))
                Browser = "Edge";
            else if (userAgent.Contains("opera"))
                Browser = "Opera";
        }

        // Override methods
        protected override bool ValidateInvariants()
        {
            return !string.IsNullOrWhiteSpace(TokenHash) && 
                   !string.IsNullOrWhiteSpace(UserId) && 
                   ExpiresAt > DateTime.UtcNow;
        }

        protected override void ValidateAggregateState()
        {
            if (IsRotated && !ReplacedById.HasValue)
                throw new InvalidOperationException("Rotated tokens must have a replacement token");

            if (IsRevoked && string.IsNullOrWhiteSpace(RevokedBy))
                throw new InvalidOperationException("Revoked tokens must have a revocation user");

            if (RotationCount < 0)
                throw new InvalidOperationException("Rotation count cannot be negative");

            if (UsageCount < 0)
                throw new InvalidOperationException("Usage count cannot be negative");
        }
    }
} 