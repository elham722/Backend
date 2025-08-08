using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Backend.Domain.Events;
using System.Collections.Generic;

namespace Backend.Domain.Entities.Common
{
    public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>>
    {
        public TId Id { get; protected set; } = default!;

        // Audit properties
        public DateTime CreatedAt { get; protected set; }
        public string CreatedBy { get; protected set; } = string.Empty;
        public DateTime? UpdatedAt { get; protected set; }
        public string? UpdatedBy { get; protected set; }
        public bool IsDeleted { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }
        public string? DeletedBy { get; protected set; }

        // Domain Events
        private List<BaseDomainEvent>? _domainEvents;
        public IReadOnlyCollection<BaseDomainEvent> DomainEvents => 
            _domainEvents?.AsReadOnly() ?? new ReadOnlyCollection<BaseDomainEvent>(Array.Empty<BaseDomainEvent>());

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }

        protected BaseEntity(TId id) : this()
        {
            Id = id;
        }

        // Domain Event Management
        protected void AddDomainEvent(BaseDomainEvent @event)
        {
            _domainEvents ??= new List<BaseDomainEvent>();
            _domainEvents.Add(@event);
        }

        public void ClearDomainEvents() => _domainEvents?.Clear();

        public bool HasDomainEvents => _domainEvents?.Count > 0;

        // Audit Methods
        protected void SetCreatedBy(string createdBy)
        {
            if (string.IsNullOrWhiteSpace(createdBy))
                throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

            CreatedBy = createdBy;
        }

        protected void SetUpdatedBy(string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(updatedBy))
                throw new ArgumentException("UpdatedBy cannot be null or empty", nameof(updatedBy));

            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        protected void MarkAsDeleted(string deletedBy)
        {
            if (string.IsNullOrWhiteSpace(deletedBy))
                throw new ArgumentException("DeletedBy cannot be null or empty", nameof(deletedBy));

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }

        protected void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }

        // Business Logic Methods
        public bool IsNew() => CreatedAt == default;
        public bool IsModified() => UpdatedAt.HasValue;
        public bool IsActive() => !IsDeleted;

        // Equality Methods
        public override bool Equals(object? obj)
        {
            return Equals(obj as BaseEntity<TId>);
        }

        public bool Equals(BaseEntity<TId>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
        {
            return EqualityComparer<BaseEntity<TId>>.Default.Equals(left, right);
        }

        public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
        {
            return !(left == right);
        }

        // Validation
        protected virtual void ValidateId(TId id)
        {
            if (id == null || id.Equals(default(TId)))
                throw new ArgumentException("Id cannot be null or default", nameof(id));
        }

        // Override methods for custom behavior
        protected virtual void OnCreated()
        {
            // Override in derived classes for custom creation logic
        }

        protected virtual void OnUpdated()
        {
            // Override in derived classes for custom update logic
        }

        protected virtual void OnDeleted()
        {
            // Override in derived classes for custom deletion logic
        }
    }
}
