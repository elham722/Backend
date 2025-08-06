using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string CreatedBy { get; private set; } = null!;
        public string? UpdatedBy { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? AssignedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public string? AssignedBy { get; private set; }
        public string? AssignmentReason { get; private set; }

        private UserRole() { } // For EF Core

        public static UserRole Create(string userId, string roleId, string? assignedBy = null, string? assignmentReason = null, DateTime? expiresAt = null, string createdBy = "System")
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role ID cannot be null or empty", nameof(roleId));

            return new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                AssignedBy = assignedBy ?? createdBy,
                AssignmentReason = assignmentReason
            };
        }

        public void UpdateAssignmentReason(string assignmentReason, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(assignmentReason))
                throw new ArgumentException("Assignment reason cannot be null or empty", nameof(assignmentReason));

            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("Updated by cannot be null or empty", nameof(updatedBy));

            AssignmentReason = assignmentReason;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void ExtendExpiration(DateTime newExpiresAt, string extendedBy)
        {
            if (string.IsNullOrWhiteSpace(extendedBy))
                throw new ArgumentException("Extended by cannot be null or empty", nameof(extendedBy));

            if (newExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException("New expiration date must be in the future", nameof(newExpiresAt));

            ExpiresAt = newExpiresAt;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = extendedBy;
        }

        public void RemoveExpiration(string removedBy)
        {
            if (string.IsNullOrWhiteSpace(removedBy))
                throw new ArgumentException("Removed by cannot be null or empty", nameof(removedBy));

            ExpiresAt = null;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = removedBy;
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

        public bool IsActiveAndNotExpired()
        {
            return IsActive && !IsExpired();
        }

        public TimeSpan? GetRemainingTime()
        {
            if (!ExpiresAt.HasValue || IsExpired())
                return null;

            return ExpiresAt.Value - DateTime.UtcNow;
        }
    }
} 