using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Models
{
    public class UserLogin : IdentityUserLogin<string>
    {
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = null!;
        public string? UpdatedBy { get; private set; }
        public bool IsActive { get; private set; }
        public string? DeviceInfo { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }

        private UserLogin() { } // For EF Core

        public static UserLogin Create(string userId, string loginProvider, string providerKey, string? deviceInfo = null, string? ipAddress = null, string? userAgent = null, string createdBy = "System")
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(loginProvider))
                throw new ArgumentException("Login provider cannot be null or empty", nameof(loginProvider));

            if (string.IsNullOrWhiteSpace(providerKey))
                throw new ArgumentException("Provider key cannot be null or empty", nameof(providerKey));

            return new UserLogin
            {
                UserId = userId,
                LoginProvider = loginProvider,
                ProviderKey = providerKey,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActive = true,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };
        }

        public void UpdateDeviceInfo(string? deviceInfo, string? ipAddress, string? userAgent, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("Updated by cannot be null or empty", nameof(updatedBy));

            DeviceInfo = deviceInfo;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
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

        public bool IsFromSameDevice(string? deviceInfo, string? ipAddress, string? userAgent)
        {
            return DeviceInfo == deviceInfo && IpAddress == ipAddress && UserAgent == userAgent;
        }

        public bool IsExpired(TimeSpan? expirationPeriod = null)
        {
            if (!expirationPeriod.HasValue)
                return false;

            return DateTime.UtcNow.Subtract(CreatedAt) > expirationPeriod.Value;
        }
    }
} 