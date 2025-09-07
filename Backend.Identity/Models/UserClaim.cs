using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Models
{
    public class UserClaim : IdentityUserClaim<string>
    {
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = null!;
        public string? UpdatedBy { get; private set; }
        public bool IsActive { get; private set; }

        private UserClaim() { } // For EF Core

        public static UserClaim Create(string userId, string claimType, string claimValue, string createdBy = "System")
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(claimType))
                throw new ArgumentException("Claim type cannot be null or empty", nameof(claimType));

            if (string.IsNullOrWhiteSpace(claimValue))
                throw new ArgumentException("Claim value cannot be null or empty", nameof(claimValue));

            return new UserClaim
            {
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActive = true
            };
        }

        public void Update(string claimValue, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(claimValue))
                throw new ArgumentException("Claim value cannot be null or empty", nameof(claimValue));

            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("Updated by cannot be null or empty", nameof(updatedBy));

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

        public bool IsExpired(TimeSpan? expirationPeriod = null)
        {
            if (!expirationPeriod.HasValue)
                return false;

            return DateTime.UtcNow.Subtract(CreatedAt) > expirationPeriod.Value;
        }
    }
} 