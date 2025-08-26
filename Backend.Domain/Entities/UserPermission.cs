using System;
using System.Collections.Generic;
using Backend.Domain.Entities.Common;
using Backend.Domain.Common;

namespace Backend.Domain.Entities
{
    public class UserPermission : BaseEntity<Guid>
    {
        // Relationships
        public string UserId { get; private set; } = null!;
        public Guid PermissionId { get; private set; }
        public Permission Permission { get; private set; } = null!;

        // Assignment Details
        public DateTime AssignedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public string AssignedBy { get; private set; } = null!;
        public string? AssignmentReason { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsInherited { get; private set; }
        public Guid? InheritedFromRoleId { get; private set; }
        public string? InheritedFromUserId { get; private set; }
        public bool IsDenied { get; private set; }
        public string? DenialReason { get; private set; }

        // Computed Properties
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        public bool IsValid => IsActive && !IsExpired && !IsDenied;
        public TimeSpan? RemainingTime => ExpiresAt.HasValue && !IsExpired ? ExpiresAt.Value - DateTime.UtcNow : null;

        private UserPermission() { } // For EF Core

        private UserPermission(Guid id, string userId, Guid permissionId, string assignedBy, string? assignmentReason = null, DateTime? expiresAt = null)
        {
            ValidateId(id);
            Guard.AgainstNullOrEmpty(userId, nameof(userId));
            Guard.AgainstEmpty(permissionId, nameof(permissionId));
            Guard.AgainstNullOrEmpty(assignedBy, nameof(assignedBy));

            Id = id;
            UserId = userId;
            PermissionId = permissionId;
            AssignedAt = DateTime.UtcNow;
            AssignedBy = assignedBy;
            AssignmentReason = assignmentReason?.Trim();
            ExpiresAt = expiresAt;
            IsActive = true;
            IsInherited = false;
            IsDenied = false;
        }

        public static UserPermission Create(string userId, Guid permissionId, string assignedBy, string? assignmentReason = null, DateTime? expiresAt = null)
        {
            return new UserPermission(Guid.NewGuid(), userId, permissionId, assignedBy, assignmentReason, expiresAt);
        }

        public static UserPermission CreateInheritedFromRole(string userId, Guid permissionId, Guid inheritedFromRoleId, string assignedBy)
        {
            var userPermission = new UserPermission(Guid.NewGuid(), userId, permissionId, assignedBy);
            userPermission.IsInherited = true;
            userPermission.InheritedFromRoleId = inheritedFromRoleId;
            return userPermission;
        }

        public static UserPermission CreateInheritedFromUser(string userId, Guid permissionId, string inheritedFromUserId, string assignedBy)
        {
            var userPermission = new UserPermission(Guid.NewGuid(), userId, permissionId, assignedBy);
            userPermission.IsInherited = true;
            userPermission.InheritedFromUserId = inheritedFromUserId;
            return userPermission;
        }

        public static UserPermission CreateDenied(string userId, Guid permissionId, string assignedBy, string denialReason)
        {
            Guard.AgainstNullOrEmpty(denialReason, nameof(denialReason));

            var userPermission = new UserPermission(Guid.NewGuid(), userId, permissionId, assignedBy);
            userPermission.IsDenied = true;
            userPermission.DenialReason = denialReason.Trim();
            return userPermission;
        }

        // Assignment Management Methods
        public void UpdateAssignmentReason(string assignmentReason, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            AssignmentReason = assignmentReason?.Trim();
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void ExtendExpiration(DateTime newExpiresAt, string extendedBy)
        {
            Guard.AgainstNullOrEmpty(extendedBy, nameof(extendedBy));

            if (newExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException("New expiration date must be in the future", nameof(newExpiresAt));

            if (IsDenied)
                throw new InvalidOperationException("Cannot extend expiration of a denied permission");

            ExpiresAt = newExpiresAt;
            SetUpdatedBy(extendedBy);
            OnUpdated();
        }

        public void RemoveExpiration(string removedBy)
        {
            Guard.AgainstNullOrEmpty(removedBy, nameof(removedBy));

            ExpiresAt = null;
            SetUpdatedBy(removedBy);
            OnUpdated();
        }

        public void Deactivate(string deactivatedBy)
        {
            Guard.AgainstNullOrEmpty(deactivatedBy, nameof(deactivatedBy));

            IsActive = false;
            SetUpdatedBy(deactivatedBy);
            OnUpdated();
        }

        public void Activate(string activatedBy)
        {
            Guard.AgainstNullOrEmpty(activatedBy, nameof(activatedBy));

            if (IsDenied)
                throw new InvalidOperationException("Cannot activate a denied permission");

            IsActive = true;
            SetUpdatedBy(activatedBy);
            OnUpdated();
        }

        public void MarkAsInheritedFromRole(Guid inheritedFromRoleId, string updatedBy)
        {
            Guard.AgainstEmpty(inheritedFromRoleId, nameof(inheritedFromRoleId));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            IsInherited = true;
            InheritedFromRoleId = inheritedFromRoleId;
            InheritedFromUserId = null;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void MarkAsInheritedFromUser(string inheritedFromUserId, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(inheritedFromUserId, nameof(inheritedFromUserId));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            IsInherited = true;
            InheritedFromUserId = inheritedFromUserId;
            InheritedFromRoleId = null;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void RemoveInheritance(string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            IsInherited = false;
            InheritedFromRoleId = null;
            InheritedFromUserId = null;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void Deny(string deniedBy, string denialReason)
        {
            Guard.AgainstNullOrEmpty(deniedBy, nameof(deniedBy));
            Guard.AgainstNullOrEmpty(denialReason, nameof(denialReason));

            IsDenied = true;
            DenialReason = denialReason.Trim();
            IsActive = false;
            SetUpdatedBy(deniedBy);
            OnUpdated();
        }

        public void RemoveDenial(string removedBy)
        {
            Guard.AgainstNullOrEmpty(removedBy, nameof(removedBy));

            IsDenied = false;
            DenialReason = null;
            IsActive = true;
            SetUpdatedBy(removedBy);
            OnUpdated();
        }

        // Business Logic Methods
        public bool CanBeModified()
        {
            return IsActive && !IsExpired && !IsDenied;
        }

        public bool CanBeDeactivated()
        {
            return IsActive && !IsDenied;
        }

        public bool CanBeDenied()
        {
            return !IsDenied;
        }

        public bool IsPermanent()
        {
            return !ExpiresAt.HasValue;
        }

        public bool IsTemporary()
        {
            return ExpiresAt.HasValue;
        }

        public bool IsExpiringSoon(int daysThreshold = 7)
        {
            if (!ExpiresAt.HasValue || IsExpired)
                return false;

            var timeUntilExpiry = ExpiresAt.Value - DateTime.UtcNow;
            return timeUntilExpiry.TotalDays <= daysThreshold;
        }

        public bool IsInheritedFromRole()
        {
            return IsInherited && InheritedFromRoleId.HasValue;
        }

        public bool IsInheritedFromUser()
        {
            return IsInherited && !string.IsNullOrWhiteSpace(InheritedFromUserId);
        }

        public string GetStatusDescription()
        {
            if (IsDenied)
                return "Denied";

            if (!IsActive)
                return "Inactive";

            if (IsExpired)
                return "Expired";

            if (IsExpiringSoon())
                return "Expiring Soon";

            if (IsInherited)
                return "Inherited";

            return "Active";
        }

        public string GetInheritanceSource()
        {
            if (!IsInherited)
                return "Direct";

            if (InheritedFromRoleId.HasValue)
                return $"Role: {InheritedFromRoleId}";

            if (!string.IsNullOrWhiteSpace(InheritedFromUserId))
                return $"User: {InheritedFromUserId}";

            return "Unknown";
        }

        // Override methods
        protected override bool ValidateInvariants()
        {
            return !string.IsNullOrWhiteSpace(UserId) && PermissionId != Guid.Empty;
        }

        protected override void ValidateAggregateState()
        {
            if (ExpiresAt.HasValue && ExpiresAt.Value <= AssignedAt)
                throw new InvalidOperationException("Expiration date must be after assignment date");

            if (IsInherited && !InheritedFromRoleId.HasValue && string.IsNullOrWhiteSpace(InheritedFromUserId))
                throw new InvalidOperationException("Inherited permissions must have a source role or user");

            if (IsInherited && InheritedFromRoleId.HasValue && !string.IsNullOrWhiteSpace(InheritedFromUserId))
                throw new InvalidOperationException("Permission cannot be inherited from both role and user");

            if (IsDenied && string.IsNullOrWhiteSpace(DenialReason))
                throw new InvalidOperationException("Denied permissions must have a reason");
        }
    }
} 