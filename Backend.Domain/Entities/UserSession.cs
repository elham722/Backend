using System;
using System.Collections.Generic;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Common;
using Backend.Domain.Events.Identity;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.Entities
{
    public class UserSession : BaseAggregateRoot<Guid>
    {
        // Basic Information
        public string UserId { get; private set; } = null!;
        public string SessionId { get; private set; } = null!;
        public DateTime StartedAt { get; private set; }
        public DateTime? LastActivityAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public DateTime? EndedAt { get; private set; }
        public string? EndedBy { get; private set; }
        public string? EndReason { get; private set; }

        // Device Information
        public string? DeviceId { get; private set; }
        public string? DeviceType { get; private set; }
        public string? DeviceName { get; private set; }
        public string? DeviceModel { get; private set; }
        public string? DeviceManufacturer { get; private set; }
        public string? OperatingSystem { get; private set; }
        public string? OsVersion { get; private set; }
        public string? Browser { get; private set; }
        public string? BrowserVersion { get; private set; }
        public string? UserAgent { get; private set; }

        // Location Information
        public string? IpAddress { get; private set; }
        public string? Country { get; private set; }
        public string? Region { get; private set; }
        public string? City { get; private set; }
        public string? TimeZone { get; private set; }
        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }

        // Security Information
        public bool IsActive { get; private set; }
        public bool IsTrusted { get; private set; }
        public bool IsRemembered { get; private set; }
        public string? TrustReason { get; private set; }
        public string? TrustedBy { get; private set; }
        public DateTime? TrustedAt { get; private set; }
        public int LoginAttempts { get; private set; }
        public DateTime? LastLoginAttemptAt { get; private set; }

        // Computed Properties
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
        public bool IsValid => IsActive && !IsExpired && EndedAt == null;
        public TimeSpan Duration => (LastActivityAt ?? DateTime.UtcNow) - StartedAt;
        public TimeSpan? IdleTime => LastActivityAt.HasValue ? DateTime.UtcNow - LastActivityAt.Value : null;
        public TimeSpan? RemainingTime => ExpiresAt.HasValue && !IsExpired ? ExpiresAt.Value - DateTime.UtcNow : null;
        public bool HasLocation => !string.IsNullOrWhiteSpace(Country) || !string.IsNullOrWhiteSpace(City);
        public bool IsMobile => DeviceType?.Equals("Mobile", StringComparison.OrdinalIgnoreCase) ?? false;

        private UserSession() { } // For EF Core

        private UserSession(Guid id, string userId, string sessionId, string? createdBy = null)
        {
            ValidateId(id);
            Guard.AgainstNullOrEmpty(userId, nameof(userId));
            Guard.AgainstNullOrEmpty(sessionId, nameof(sessionId));

            Id = id;
            UserId = userId;
            SessionId = sessionId;
            StartedAt = DateTime.UtcNow;
            LastActivityAt = DateTime.UtcNow;
            IsActive = true;
            IsTrusted = false;
            IsRemembered = false;
            LoginAttempts = 0;

            if (!string.IsNullOrWhiteSpace(createdBy))
                SetCreatedBy(createdBy);
        }

        public static UserSession Create(string userId, string sessionId, string? createdBy = null)
        {
            var session = new UserSession(Guid.NewGuid(), userId, sessionId, createdBy);
            session.AddDomainEvent(new UserSessionStartedEvent(session.Id, session.UserId, session.SessionId));
            session.OnCreated();
            return session;
        }

        public static UserSession CreateWithDeviceInfo(string userId, string sessionId, 
            string? deviceType = null, string? deviceName = null, string? operatingSystem = null, 
            string? browser = null, string? ipAddress = null, string? createdBy = null)
        {
            var session = Create(userId, sessionId, createdBy);
            session.SetDeviceInfo(deviceType, deviceName, operatingSystem, browser, ipAddress);
            return session;
        }

        // Device Information Methods
        public void SetDeviceInfo(string? deviceType, string? deviceName, string? operatingSystem, 
            string? browser, string? ipAddress)
        {
            DeviceType = deviceType?.Trim();
            DeviceName = deviceName?.Trim();
            OperatingSystem = operatingSystem?.Trim();
            Browser = browser?.Trim();
            IpAddress = ipAddress?.Trim();
            
            OnUpdated();
        }

        public void SetDeviceInfo(string? deviceId, string? deviceType, string? deviceName, 
            string? deviceModel, string? deviceManufacturer, string? operatingSystem, string? osVersion,
            string? browser, string? browserVersion, string? userAgent)
        {
            DeviceId = deviceId?.Trim();
            DeviceType = deviceType?.Trim();
            DeviceName = deviceName?.Trim();
            DeviceModel = deviceModel?.Trim();
            DeviceManufacturer = deviceManufacturer?.Trim();
            OperatingSystem = operatingSystem?.Trim();
            OsVersion = osVersion?.Trim();
            Browser = browser?.Trim();
            BrowserVersion = browserVersion?.Trim();
            UserAgent = userAgent?.Trim();
            
            OnUpdated();
        }

        public void UpdateDeviceInfo(string? deviceType, string? deviceName, string? operatingSystem, 
            string? browser, string? ipAddress, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            SetDeviceInfo(deviceType, deviceName, operatingSystem, browser, ipAddress);
            SetUpdatedBy(updatedBy);
        }

        // Location Information Methods
        public void SetLocationInfo(string? country, string? region, string? city, string? timeZone, 
            double? latitude = null, double? longitude = null)
        {
            Country = country?.Trim();
            Region = region?.Trim();
            City = city?.Trim();
            TimeZone = timeZone?.Trim();
            Latitude = latitude;
            Longitude = longitude;
            
            OnUpdated();
        }

        public void UpdateLocationInfo(string? country, string? region, string? city, string? timeZone, 
            double? latitude = null, double? longitude = null, string updatedBy = "System")
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            SetLocationInfo(country, region, city, timeZone, latitude, longitude);
            SetUpdatedBy(updatedBy);
        }

        // Session Management Methods
        public void UpdateActivity()
        {
            LastActivityAt = DateTime.UtcNow;
            OnUpdated();
        }

        public void ExtendExpiration(DateTime newExpiresAt, string extendedBy)
        {
            Guard.AgainstNullOrEmpty(extendedBy, nameof(extendedBy));

            if (newExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException("New expiration date must be in the future", nameof(newExpiresAt));

            if (!IsActive)
                throw new InvalidOperationException("Cannot extend expiration of an inactive session");

            var oldExpiresAt = ExpiresAt;
            ExpiresAt = newExpiresAt;
            SetUpdatedBy(extendedBy);
            OnUpdated();
            
            AddDomainEvent(new UserSessionExtendedEvent(Id, UserId, SessionId, oldExpiresAt, ExpiresAt));
        }

        public void SetExpiration(DateTime expiresAt, string setBy)
        {
            Guard.AgainstNullOrEmpty(setBy, nameof(setBy));

            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

            ExpiresAt = expiresAt;
            SetUpdatedBy(setBy);
            OnUpdated();
        }

        public void RemoveExpiration(string removedBy)
        {
            Guard.AgainstNullOrEmpty(removedBy, nameof(removedBy));

            ExpiresAt = null;
            SetUpdatedBy(removedBy);
            OnUpdated();
        }

        public void End(string endedBy, string? endReason = null)
        {
            Guard.AgainstNullOrEmpty(endedBy, nameof(endedBy));

            if (!IsActive)
                throw new InvalidOperationException("Session is already ended");

            EndedAt = DateTime.UtcNow;
            EndedBy = endedBy;
            EndReason = endReason?.Trim();
            IsActive = false;
            SetUpdatedBy(endedBy);
            OnUpdated();
            
            AddDomainEvent(new UserSessionEndedEvent(Id, UserId, SessionId, EndReason));
        }

        public void Deactivate(string deactivatedBy, string? reason = null)
        {
            Guard.AgainstNullOrEmpty(deactivatedBy, nameof(deactivatedBy));

            if (!IsActive)
                return;

            IsActive = false;
            SetUpdatedBy(deactivatedBy);
            OnUpdated();
            
            AddDomainEvent(new UserSessionDeactivatedEvent(Id, UserId, SessionId, reason));
        }

        public void Activate(string activatedBy)
        {
            Guard.AgainstNullOrEmpty(activatedBy, nameof(activatedBy));

            if (IsActive)
                return;

            IsActive = true;
            SetUpdatedBy(activatedBy);
            OnUpdated();
            
            AddDomainEvent(new UserSessionActivatedEvent(Id, UserId, SessionId));
        }

        // Trust Management Methods
        public void MarkAsTrusted(string trustedBy, string? trustReason = null)
        {
            Guard.AgainstNullOrEmpty(trustedBy, nameof(trustedBy));

            if (IsTrusted)
                return;

            IsTrusted = true;
            TrustReason = trustReason?.Trim();
            TrustedBy = trustedBy;
            TrustedAt = DateTime.UtcNow;
            SetUpdatedBy(trustedBy);
            OnUpdated();
            
            AddDomainEvent(new UserSessionTrustedEvent(Id, UserId, SessionId, TrustReason));
        }

        public void RemoveTrust(string removedBy, string? reason = null)
        {
            Guard.AgainstNullOrEmpty(removedBy, nameof(removedBy));

            if (!IsTrusted)
                return;

            IsTrusted = false;
            TrustReason = null;
            TrustedBy = null;
            TrustedAt = null;
            SetUpdatedBy(removedBy);
            OnUpdated();
            
            AddDomainEvent(new UserSessionTrustRemovedEvent(Id, UserId, SessionId, reason));
        }

        public void SetRemembered(bool isRemembered, string setBy)
        {
            Guard.AgainstNullOrEmpty(setBy, nameof(setBy));

            IsRemembered = isRemembered;
            SetUpdatedBy(setBy);
            OnUpdated();
        }

        // Security Methods
        public void IncrementLoginAttempts()
        {
            LoginAttempts++;
            LastLoginAttemptAt = DateTime.UtcNow;
            OnUpdated();
        }

        public void ResetLoginAttempts()
        {
            LoginAttempts = 0;
            LastLoginAttemptAt = null;
            OnUpdated();
        }

        public void BlockDueToFailedAttempts(string blockedBy, int maxAttempts = 5)
        {
            Guard.AgainstNullOrEmpty(blockedBy, nameof(blockedBy));

            if (LoginAttempts >= maxAttempts)
            {
                Deactivate(blockedBy, "Blocked due to multiple failed login attempts");
            }
        }

        // Business Logic Methods
        public bool CanBeExtended()
        {
            return IsActive && !IsExpired;
        }

        public bool CanBeTrusted()
        {
            return IsActive && !IsTrusted;
        }

        public bool CanBeEnded()
        {
            return IsActive;
        }

        public bool IsIdle(TimeSpan idleThreshold)
        {
            return LastActivityAt.HasValue && (DateTime.UtcNow - LastActivityAt.Value) > idleThreshold;
        }

        public bool IsFromSameDevice(string? deviceType, string? deviceName, string? operatingSystem, string? browser)
        {
            var deviceTypeMatch = string.IsNullOrWhiteSpace(DeviceType) || 
                                 string.IsNullOrWhiteSpace(deviceType) || 
                                 DeviceType.Equals(deviceType, StringComparison.OrdinalIgnoreCase);

            var deviceNameMatch = string.IsNullOrWhiteSpace(DeviceName) || 
                                 string.IsNullOrWhiteSpace(deviceName) || 
                                 DeviceName.Equals(deviceName, StringComparison.OrdinalIgnoreCase);

            var osMatch = string.IsNullOrWhiteSpace(OperatingSystem) || 
                         string.IsNullOrWhiteSpace(operatingSystem) || 
                         OperatingSystem.Equals(operatingSystem, StringComparison.OrdinalIgnoreCase);

            var browserMatch = string.IsNullOrWhiteSpace(Browser) || 
                              string.IsNullOrWhiteSpace(browser) || 
                              Browser.Equals(browser, StringComparison.OrdinalIgnoreCase);

            return deviceTypeMatch && deviceNameMatch && osMatch && browserMatch;
        }

        public bool IsFromSameLocation(string? country, string? city)
        {
            var countryMatch = string.IsNullOrWhiteSpace(Country) || 
                              string.IsNullOrWhiteSpace(country) || 
                              Country.Equals(country, StringComparison.OrdinalIgnoreCase);

            var cityMatch = string.IsNullOrWhiteSpace(City) || 
                           string.IsNullOrWhiteSpace(city) || 
                           City.Equals(city, StringComparison.OrdinalIgnoreCase);

            return countryMatch && cityMatch;
        }

        public string GetStatusDescription()
        {
            if (EndedAt.HasValue)
                return "Ended";

            if (!IsActive)
                return "Inactive";

            if (IsExpired)
                return "Expired";

            if (IsTrusted)
                return "Trusted";

            if (IsRemembered)
                return "Remembered";

            return "Active";
        }

        public string GetDeviceDescription()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(DeviceType))
                parts.Add(DeviceType);

            if (!string.IsNullOrWhiteSpace(DeviceManufacturer))
                parts.Add(DeviceManufacturer);

            if (!string.IsNullOrWhiteSpace(DeviceModel))
                parts.Add(DeviceModel);

            if (!string.IsNullOrWhiteSpace(OperatingSystem))
            {
                var os = OperatingSystem;
                if (!string.IsNullOrWhiteSpace(OsVersion))
                    os += $" {OsVersion}";
                parts.Add(os);
            }

            if (!string.IsNullOrWhiteSpace(Browser))
            {
                var browser = Browser;
                if (!string.IsNullOrWhiteSpace(BrowserVersion))
                    browser += $" {BrowserVersion}";
                parts.Add(browser);
            }

            if (parts.Count == 0)
                return "Unknown Device";

            return string.Join(" - ", parts);
        }

        public string GetLocationDescription()
        {
            if (!HasLocation)
                return "Unknown Location";

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(City))
                parts.Add(City);

            if (!string.IsNullOrWhiteSpace(Region))
                parts.Add(Region);

            if (!string.IsNullOrWhiteSpace(Country))
                parts.Add(Country);

            if (parts.Count == 0)
                return "Unknown Location";

            return string.Join(", ", parts);
        }

        // Override methods
        protected override bool ValidateInvariants()
        {
            return !string.IsNullOrWhiteSpace(UserId) && 
                   !string.IsNullOrWhiteSpace(SessionId) && 
                   StartedAt <= DateTime.UtcNow;
        }

        protected override void ValidateAggregateState()
        {
            if (ExpiresAt.HasValue && ExpiresAt.Value <= StartedAt)
                throw new InvalidOperationException("Expiration date must be after start date");

            if (EndedAt.HasValue && EndedAt.Value < StartedAt)
                throw new InvalidOperationException("End date cannot be before start date");

            if (LastActivityAt.HasValue && LastActivityAt.Value < StartedAt)
                throw new InvalidOperationException("Last activity cannot be before start date");

            if (TrustedAt.HasValue && TrustedAt.Value < StartedAt)
                throw new InvalidOperationException("Trust date cannot be before start date");

            if (LoginAttempts < 0)
                throw new InvalidOperationException("Login attempts cannot be negative");

            if (Latitude.HasValue && (Latitude.Value < -90 || Latitude.Value > 90))
                throw new InvalidOperationException("Latitude must be between -90 and 90");

            if (Longitude.HasValue && (Longitude.Value < -180 || Longitude.Value > 180))
                throw new InvalidOperationException("Longitude must be between -180 and 180");
        }
    }
} 