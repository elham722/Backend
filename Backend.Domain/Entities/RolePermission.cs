using System;
using System.Collections.Generic;
using Backend.Domain.Entities.Common;
using Backend.Domain.Common;

namespace Backend.Domain.Entities
{
    public class RolePermission : BaseEntity<Guid>
    {
        // Relationships
        public Guid RoleId { get; private set; }
        public Role Role { get; private set; } = null!;
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
        public Role? InheritedFromRole { get; private set; }

        // Computed Properties
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        public bool IsValid => IsActive && !IsExpired;
        public TimeSpan? RemainingTime => ExpiresAt.HasValue && !IsExpired ? ExpiresAt.Value - DateTime.UtcNow : null;

        private RolePermission() { } // For EF Core

        private RolePermission(Guid id, Guid roleId, Guid permissionId, string assignedBy, string? assignmentReason = null, DateTime? expiresAt = null)
        {
            ValidateId(id);
            Guard.AgainstEmpty(roleId, nameof(roleId));
            Guard.AgainstEmpty(permissionId, nameof(permissionId));
            Guard.AgainstNullOrEmpty(assignedBy, nameof(assignedBy));

            Id = id;
            RoleId = roleId;
            PermissionId = permissionId;
            AssignedAt = DateTime.UtcNow;
            AssignedBy = assignedBy;
            AssignmentReason = assignmentReason?.Trim();
            ExpiresAt = expiresAt;
            IsActive = true;
            IsInherited = false;
        }

        public static RolePermission Create(Guid roleId, Guid permissionId, string assignedBy, string? assignmentReason = null, DateTime? expiresAt = null)
        {
            return new RolePermission(Guid.NewGuid(), roleId, permissionId, assignedBy, assignmentReason, expiresAt);
        }

        public static RolePermission CreateInherited(Guid roleId, Guid permissionId, Guid inheritedFromRoleId, string assignedBy)
        {
            var rolePermission = new RolePermission(Guid.NewGuid(), roleId, permissionId, assignedBy);
            rolePermission.IsInherited = true;
            rolePermission.InheritedFromRoleId = inheritedFromRoleId;
            return rolePermission;
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

            IsActive = true;
            SetUpdatedBy(activatedBy);
            OnUpdated();
        }

        public void MarkAsInherited(Guid inheritedFromRoleId, string updatedBy)
        {
            Guard.AgainstEmpty(inheritedFromRoleId, nameof(inheritedFromRoleId));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            IsInherited = true;
            InheritedFromRoleId = inheritedFromRoleId;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void RemoveInheritance(string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            IsInherited = false;
            InheritedFromRoleId = null;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        // Business Logic Methods
        public bool CanBeModified()
        {
            return IsActive && !IsExpired;
        }

        public bool CanBeDeactivated()
        {
            return IsActive;
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

        public string GetStatusDescription()
        {
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

        // Override methods
        protected override bool ValidateInvariants()
        {
            return RoleId != Guid.Empty && PermissionId != Guid.Empty;
        }

        protected override void ValidateAggregateState()
        {
            if (ExpiresAt.HasValue && ExpiresAt.Value <= AssignedAt)
                throw new InvalidOperationException("Expiration date must be after assignment date");

            if (IsInherited && !InheritedFromRoleId.HasValue)
                throw new InvalidOperationException("Inherited permissions must have a source role");
        }
    }
} 