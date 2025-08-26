using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Common;
using Backend.Domain.Events.Identity;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities
{
    public class Role : BaseAggregateRoot<Guid>
    {
        // Basic Information
        public string Name { get; private set; } = null!;
        public string? DisplayName { get; private set; }
        public string? Description { get; private set; }
        public EntityStatus Status { get; private set; }
        public RoleType RoleType { get; private set; }
        public int Priority { get; private set; }
        public bool IsSystemRole { get; private set; }
        public bool IsDefault { get; private set; }

        // Role Hierarchy
        public Guid? ParentRoleId { get; private set; }
        public Role? ParentRole { get; private set; }
        public ICollection<Role> ChildRoles { get; private set; } = new List<Role>();

        // Role Assignments
        public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

        // Computed Properties
        public bool IsActive => Status == EntityStatus.Active;
        public bool IsBuiltIn => IsSystemRole || IsDefault;
        public bool HasParent => ParentRoleId.HasValue;
        public bool HasChildren => ChildRoles.Any();
        public int Depth => CalculateDepth();
        public string FullName => HasParent ? $"{ParentRole?.FullName} > {Name}" : Name;

        private Role() { } // For EF Core

        private Role(Guid id, string name, string? createdBy = null)
        {
            ValidateId(id);
            Guard.AgainstNullOrEmpty(name, nameof(name));

            Id = id;
            Name = name.Trim();
            Status = EntityStatus.Active;
            RoleType = RoleType.Custom;
            Priority = 0;
            IsSystemRole = false;
            IsDefault = false;

            if (!string.IsNullOrWhiteSpace(createdBy))
                SetCreatedBy(createdBy);
        }

        public static Role Create(string name, string? displayName = null, string? description = null, string? createdBy = null)
        {
            var role = new Role(Guid.NewGuid(), name, createdBy);
            role.DisplayName = displayName?.Trim();
            role.Description = description?.Trim();
            
            role.AddDomainEvent(new RoleCreatedEvent(role.Id, role.Name));
            role.OnCreated();
            return role;
        }

        public static Role CreateSystem(string name, string? displayName = null, string? description = null, int priority = 0)
        {
            var role = new Role(Guid.NewGuid(), name, "System");
            role.DisplayName = displayName?.Trim();
            role.Description = description?.Trim();
            role.RoleType = RoleType.System;
            role.Priority = priority;
            role.IsSystemRole = true;
            role.IsDefault = false;
            
            role.AddDomainEvent(new RoleCreatedEvent(role.Id, role.Name));
            role.OnCreated();
            return role;
        }

        public static Role CreateDefault(string name, string? displayName = null, string? description = null, int priority = 0)
        {
            var role = new Role(Guid.NewGuid(), name, "System");
            role.DisplayName = displayName?.Trim();
            role.Description = description?.Trim();
            role.RoleType = RoleType.Default;
            role.Priority = priority;
            role.IsSystemRole = false;
            role.IsDefault = true;
            
            role.AddDomainEvent(new RoleCreatedEvent(role.Id, role.Name));
            role.OnCreated();
            return role;
        }

        // Basic Information Methods
        public void UpdateName(string name, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (IsBuiltIn)
                throw new InvalidOperationException("Cannot modify built-in roles");

            var oldName = Name;
            Name = name.Trim();
            SetUpdatedBy(updatedBy);
            OnUpdated();
            
            AddDomainEvent(new RoleNameChangedEvent(Id, oldName, Name));
        }

        public void UpdateDisplayName(string? displayName, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            DisplayName = displayName?.Trim();
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void UpdateDescription(string? description, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            Description = description?.Trim();
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void UpdatePriority(int priority, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));
            Guard.AgainstNegative(priority, nameof(priority));

            Priority = priority;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        // Role Hierarchy Methods
        public void SetParentRole(Role? parentRole, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (parentRole != null && parentRole.Id == Id)
                throw new InvalidOperationException("Role cannot be its own parent");

            if (parentRole != null && WouldCreateCircularReference(parentRole))
                throw new InvalidOperationException("Setting this parent would create a circular reference");

            ParentRoleId = parentRole?.Id;
            ParentRole = parentRole;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void AddChildRole(Role childRole, string updatedBy)
        {
            Guard.AgainstNull(childRole, nameof(childRole));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (childRole.Id == Id)
                throw new InvalidOperationException("Role cannot be its own child");

            if (ChildRoles.Any(r => r.Id == childRole.Id))
                return; // Already a child

            ChildRoles.Add(childRole);
            childRole.SetParentRole(this, updatedBy);
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void RemoveChildRole(Role childRole, string updatedBy)
        {
            Guard.AgainstNull(childRole, nameof(childRole));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (ChildRoles.Remove(childRole))
            {
                childRole.SetParentRole(null, updatedBy);
                SetUpdatedBy(updatedBy);
                OnUpdated();
            }
        }

        // Status Management Methods
        public void Activate(string activatedBy)
        {
            Guard.AgainstNullOrEmpty(activatedBy, nameof(activatedBy));

            if (Status == EntityStatus.Active)
                return;

            Status = EntityStatus.Active;
            SetUpdatedBy(activatedBy);
            OnUpdated();
            
            AddDomainEvent(new RoleActivatedEvent(Id, Name));
        }

        public void Deactivate(string deactivatedBy)
        {
            Guard.AgainstNullOrEmpty(deactivatedBy, nameof(deactivatedBy));

            if (IsBuiltIn)
                throw new InvalidOperationException("Cannot deactivate built-in roles");

            if (Status == EntityStatus.Inactive)
                return;

            Status = EntityStatus.Inactive;
            SetUpdatedBy(deactivatedBy);
            OnUpdated();
            
            AddDomainEvent(new RoleDeactivatedEvent(Id, Name));
        }

        public void MarkAsDeleted(string deletedBy)
        {
            Guard.AgainstNullOrEmpty(deletedBy, nameof(deletedBy));

            if (IsBuiltIn)
                throw new InvalidOperationException("Cannot delete built-in roles");

            if (HasChildren)
                throw new InvalidOperationException("Cannot delete role with child roles");

            Status = EntityStatus.Deleted;
            MarkAsDeleted(deletedBy);
            OnDeleted();
            
            AddDomainEvent(new RoleDeletedEvent(Id, Name));
        }

        // Business Logic Methods
        public bool CanBeModified()
        {
            return !IsBuiltIn && Status != EntityStatus.Deleted;
        }

        public bool CanBeDeleted()
        {
            return !IsBuiltIn && !HasChildren && Status != EntityStatus.Deleted;
        }

        public bool IsDescendantOf(Role ancestor)
        {
            if (ancestor == null || !HasParent)
                return false;

            if (ParentRoleId == ancestor.Id)
                return true;

            return ParentRole?.IsDescendantOf(ancestor) ?? false;
        }

        public bool IsAncestorOf(Role descendant)
        {
            return descendant?.IsDescendantOf(this) ?? false;
        }

        public IEnumerable<Role> GetAncestors()
        {
            var ancestors = new List<Role>();
            var current = ParentRole;

            while (current != null)
            {
                ancestors.Add(current);
                current = current.ParentRole;
            }

            return ancestors;
        }

        public IEnumerable<Role> GetDescendants()
        {
            var descendants = new List<Role>();

            foreach (var child in ChildRoles)
            {
                descendants.Add(child);
                descendants.AddRange(child.GetDescendants());
            }

            return descendants;
        }

        public IEnumerable<Role> GetSiblings()
        {
            if (!HasParent)
                return Enumerable.Empty<Role>();

            return ParentRole!.ChildRoles.Where(r => r.Id != Id);
        }

        public bool HasPermission(string permissionName)
        {
            return RolePermissions.Any(rp => rp.Permission.Name == permissionName && rp.IsActive);
        }

        public bool HasAnyPermission(IEnumerable<string> permissionNames)
        {
            return permissionNames.Any(HasPermission);
        }

        public bool HasAllPermissions(IEnumerable<string> permissionNames)
        {
            return permissionNames.All(HasPermission);
        }

        // Helper Methods
        private bool WouldCreateCircularReference(Role potentialParent)
        {
            if (potentialParent.Id == Id)
                return true;

            return potentialParent.IsDescendantOf(this);
        }

        private int CalculateDepth()
        {
            if (!HasParent)
                return 0;

            return 1 + (ParentRole?.CalculateDepth() ?? 0);
        }

        // Override methods
        protected override bool ValidateInvariants()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        protected override void ValidateAggregateState()
        {
            if (IsBuiltIn && Status == EntityStatus.Deleted)
                throw new InvalidOperationException("Built-in roles cannot be deleted");

            if (HasParent && ParentRole == null)
                throw new InvalidOperationException("Parent role reference is invalid");

            if (Priority < 0)
                throw new InvalidOperationException("Priority cannot be negative");
        }
    }

    public enum RoleType
    {
        Custom,
        System,
        Default
    }
} 