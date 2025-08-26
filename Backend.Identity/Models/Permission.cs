using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Common;
using Backend.Domain.Events.Identity;
using Backend.Domain.Enums;

namespace Backend.Domain.Entities
{
    public class Permission : BaseAggregateRoot<Guid>
    {
        // Basic Information
        public string Name { get; private set; } = null!;
        public string? DisplayName { get; private set; }
        public string? Description { get; private set; }
        public EntityStatus Status { get; private set; }
        public PermissionType PermissionType { get; private set; }
        public string? Resource { get; private set; }
        public string? Action { get; private set; }
        public int Priority { get; private set; }
        public bool IsSystemPermission { get; private set; }

        // Permission Hierarchy
        public Guid? ParentPermissionId { get; private set; }
        public Permission? ParentPermission { get; private set; }
        public ICollection<Permission> ChildPermissions { get; private set; } = new List<Permission>();

        // Permission Assignments
        public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();
        public ICollection<UserPermission> UserPermissions { get; private set; } = new List<UserPermission>();

        // Computed Properties
        public bool IsActive => Status == EntityStatus.Active;
        public bool IsBuiltIn => IsSystemPermission;
        public bool HasParent => ParentPermissionId.HasValue;
        public bool HasChildren => ChildPermissions.Any();
        public int Depth => CalculateDepth();
        public string FullName => HasParent ? $"{ParentPermission?.FullName} > {Name}" : Name;
        public string ResourceAction => !string.IsNullOrWhiteSpace(Resource) && !string.IsNullOrWhiteSpace(Action) 
            ? $"{Resource}:{Action}" 
            : Name;

        private Permission() { } // For EF Core

        private Permission(Guid id, string name, string? createdBy = null)
        {
            ValidateId(id);
            Guard.AgainstNullOrEmpty(name, nameof(name));

            Id = id;
            Name = name.Trim();
            Status = EntityStatus.Active;
            PermissionType = PermissionType.Custom;
            Priority = 0;
            IsSystemPermission = false;

            if (!string.IsNullOrWhiteSpace(createdBy))
                SetCreatedBy(createdBy);
        }

        public static Permission Create(string name, string? displayName = null, string? description = null, 
            string? resource = null, string? action = null, string? createdBy = null)
        {
            var permission = new Permission(Guid.NewGuid(), name, createdBy);
            permission.DisplayName = displayName?.Trim();
            permission.Description = description?.Trim();
            permission.Resource = resource?.Trim();
            permission.Action = action?.Trim();
            
            permission.AddDomainEvent(new PermissionCreatedEvent(permission.Id, permission.Name));
            permission.OnCreated();
            return permission;
        }

        public static Permission CreateSystem(string name, string? displayName = null, string? description = null,
            string? resource = null, string? action = null, int priority = 0)
        {
            var permission = new Permission(Guid.NewGuid(), name, "System");
            permission.DisplayName = displayName?.Trim();
            permission.Description = description?.Trim();
            permission.Resource = resource?.Trim();
            permission.Action = action?.Trim();
            permission.PermissionType = PermissionType.System;
            permission.Priority = priority;
            permission.IsSystemPermission = true;
            
            permission.AddDomainEvent(new PermissionCreatedEvent(permission.Id, permission.Name));
            permission.OnCreated();
            return permission;
        }

        public static Permission CreateResourcePermission(string resource, string action, string? displayName = null, 
            string? description = null, string? createdBy = null)
        {
            Guard.AgainstNullOrEmpty(resource, nameof(resource));
            Guard.AgainstNullOrEmpty(action, nameof(action));

            var name = $"{resource}:{action}";
            var permission = new Permission(Guid.NewGuid(), name, createdBy);
            permission.DisplayName = displayName?.Trim() ?? $"{action} {resource}";
            permission.Description = description?.Trim();
            permission.Resource = resource.Trim();
            permission.Action = action.Trim();
            permission.PermissionType = PermissionType.Resource;
            
            permission.AddDomainEvent(new PermissionCreatedEvent(permission.Id, permission.Name));
            permission.OnCreated();
            return permission;
        }

        // Basic Information Methods
        public void UpdateName(string name, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(name, nameof(name));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (IsBuiltIn)
                throw new InvalidOperationException("Cannot modify built-in permissions");

            var oldName = Name;
            Name = name.Trim();
            SetUpdatedBy(updatedBy);
            OnUpdated();
            
            AddDomainEvent(new PermissionNameChangedEvent(Id, oldName, Name));
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

        public void UpdateResource(string? resource, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            Resource = resource?.Trim();
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void UpdateAction(string? action, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            Action = action?.Trim();
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

        // Permission Hierarchy Methods
        public void SetParentPermission(Permission? parentPermission, string updatedBy)
        {
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (parentPermission != null && parentPermission.Id == Id)
                throw new InvalidOperationException("Permission cannot be its own parent");

            if (parentPermission != null && WouldCreateCircularReference(parentPermission))
                throw new InvalidOperationException("Setting this parent would create a circular reference");

            ParentPermissionId = parentPermission?.Id;
            ParentPermission = parentPermission;
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void AddChildPermission(Permission childPermission, string updatedBy)
        {
            Guard.AgainstNull(childPermission, nameof(childPermission));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (childPermission.Id == Id)
                throw new InvalidOperationException("Permission cannot be its own child");

            if (ChildPermissions.Any(p => p.Id == childPermission.Id))
                return; // Already a child

            ChildPermissions.Add(childPermission);
            childPermission.SetParentPermission(this, updatedBy);
            SetUpdatedBy(updatedBy);
            OnUpdated();
        }

        public void RemoveChildPermission(Permission childPermission, string updatedBy)
        {
            Guard.AgainstNull(childPermission, nameof(childPermission));
            Guard.AgainstNullOrEmpty(updatedBy, nameof(updatedBy));

            if (ChildPermissions.Remove(childPermission))
            {
                childPermission.SetParentPermission(null, updatedBy);
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
            
            AddDomainEvent(new PermissionActivatedEvent(Id, Name));
        }

        public void Deactivate(string deactivatedBy)
        {
            Guard.AgainstNullOrEmpty(deactivatedBy, nameof(deactivatedBy));

            if (IsBuiltIn)
                throw new InvalidOperationException("Cannot deactivate built-in permissions");

            if (Status == EntityStatus.Inactive)
                return;

            Status = EntityStatus.Inactive;
            SetUpdatedBy(deactivatedBy);
            OnUpdated();
            
            AddDomainEvent(new PermissionDeactivatedEvent(Id, Name));
        }

        public void MarkAsDeleted(string deletedBy)
        {
            Guard.AgainstNullOrEmpty(deletedBy, nameof(deactivatedBy));

            if (IsBuiltIn)
                throw new InvalidOperationException("Cannot delete built-in permissions");

            if (HasChildren)
                throw new InvalidOperationException("Cannot delete permission with child permissions");

            Status = EntityStatus.Deleted;
            MarkAsDeleted(deletedBy);
            OnDeleted();
            
            AddDomainEvent(new PermissionDeletedEvent(Id, Name));
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

        public bool IsDescendantOf(Permission ancestor)
        {
            if (ancestor == null || !HasParent)
                return false;

            if (ParentPermissionId == ancestor.Id)
                return true;

            return ParentPermission?.IsDescendantOf(ancestor) ?? false;
        }

        public bool IsAncestorOf(Permission descendant)
        {
            return descendant?.IsDescendantOf(this) ?? false;
        }

        public IEnumerable<Permission> GetAncestors()
        {
            var ancestors = new List<Permission>();
            var current = ParentPermission;

            while (current != null)
            {
                ancestors.Add(current);
                current = current.ParentPermission;
            }

            return ancestors;
        }

        public IEnumerable<Permission> GetDescendants()
        {
            var descendants = new List<Permission>();

            foreach (var child in ChildPermissions)
            {
                descendants.Add(child);
                descendants.AddRange(child.GetDescendants());
            }

            return descendants;
        }

        public IEnumerable<Permission> GetSiblings()
        {
            if (!HasParent)
                return Enumerable.Empty<Permission>();

            return ParentPermission!.ChildPermissions.Where(p => p.Id != Id);
        }

        public bool MatchesResourceAction(string resource, string action)
        {
            if (string.IsNullOrWhiteSpace(Resource) || string.IsNullOrWhiteSpace(Action))
                return false;

            return Resource.Equals(resource, StringComparison.OrdinalIgnoreCase) &&
                   Action.Equals(action, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsResourcePermission()
        {
            return !string.IsNullOrWhiteSpace(Resource) && !string.IsNullOrWhiteSpace(Action);
        }

        public bool IsWildcardPermission()
        {
            return Resource == "*" || Action == "*";
        }

        public bool Implies(Permission other)
        {
            if (other == null)
                return false;

            // Wildcard permissions imply everything
            if (IsWildcardPermission())
                return true;

            // Resource permissions imply matching resource actions
            if (IsResourcePermission() && other.IsResourcePermission())
            {
                if (Resource == "*" || other.Resource == "*")
                    return true;

                if (Action == "*" || other.Action == "*")
                    return Resource.Equals(other.Resource, StringComparison.OrdinalIgnoreCase);

                return MatchesResourceAction(other.Resource, other.Action);
            }

            // Name-based permissions
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        // Helper Methods
        private bool WouldCreateCircularReference(Permission potentialParent)
        {
            if (potentialParent.Id == Id)
                return true;

            return potentialParent.IsDescendantOf(this);
        }

        private int CalculateDepth()
        {
            if (!HasParent)
                return 0;

            return 1 + (ParentPermission?.CalculateDepth() ?? 0);
        }

        // Override methods
        protected override bool ValidateInvariants()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        protected override void ValidateAggregateState()
        {
            if (IsBuiltIn && Status == EntityStatus.Deleted)
                throw new InvalidOperationException("Built-in permissions cannot be deleted");

            if (HasParent && ParentPermission == null)
                throw new InvalidOperationException("Parent permission reference is invalid");

            if (Priority < 0)
                throw new InvalidOperationException("Priority cannot be negative");

            if (IsResourcePermission() && (string.IsNullOrWhiteSpace(Resource) || string.IsNullOrWhiteSpace(Action)))
                throw new InvalidOperationException("Resource permissions must have both resource and action specified");
        }
    }

    public enum PermissionType
    {
        Custom,
        System,
        Resource
    }
} 