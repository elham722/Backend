using Backend.Domain.Events;
using System.Collections.ObjectModel;

namespace Backend.Domain.Entities.Common 
{
    public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>> where TId : IEquatable<TId> // اضافه کردن constraint برای TId
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
            _domainEvents?.AsReadOnly() ?? new ReadOnlyCollection<BaseDomainEvent>(new List<BaseDomainEvent>());

        protected BaseEntity()
        {
            // CreatedAt رو اینجا ست نکن – بهتره در Application ست بشه برای تست‌پذیری
        }

        protected BaseEntity(TId id) : this()
        {
            ValidateId(id);
            Id = id;
        }

        // Domain Event Management
        protected void AddDomainEvent(BaseDomainEvent @event)
        {
            _domainEvents ??= new List<BaseDomainEvent>();
            _domainEvents.Add(@event ?? throw new ArgumentNullException(nameof(@event)));
        }

        public void ClearDomainEvents() => _domainEvents?.Clear();

        public bool HasDomainEvents => (_domainEvents?.Count ?? 0) > 0; // به property تبدیل شد برای simplicity

        // Audit Methods (با validation تمیزتر)
        public void SetCreatedBy(string createdBy)
        {
            CreatedBy = ValidateString(createdBy, nameof(createdBy));
            CreatedAt = DateTime.UtcNow; // اگر می‌خوای اینجا ست کن، اما پیشنهاد: در handler
        }

        public void SetUpdatedBy(string updatedBy)
        {
            UpdatedBy = ValidateString(updatedBy, nameof(updatedBy));
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsDeleted(string deletedBy)
        {
            DeletedBy = ValidateString(deletedBy, nameof(deletedBy));
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }

        // Private validation helper
        private static string ValidateString(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
            return value;
        }

        // Public methods for EF Core
        public void SetCreatedAt(DateTime createdAt) => CreatedAt = createdAt;
        public void SetUpdatedAt(DateTime updatedAt) => UpdatedAt = updatedAt;

        // Business Logic Methods
        public bool IsNew() => EqualityComparer<TId>.Default.Equals(Id, default!); // فیکس: بر اساس Id
        public bool IsModified() => UpdatedAt.HasValue;
        public bool IsActive() => !IsDeleted;

        // Equality Methods (همون قبلی، خوبه)
        public override bool Equals(object? obj) => Equals(obj as BaseEntity<TId>);
        public bool Equals(BaseEntity<TId>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

        public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right) => Equals(left, right);
        public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right) => !Equals(left, right);

        // Validation
        protected virtual void ValidateId(TId id)
        {
            if (EqualityComparer<TId>.Default.Equals(id, default!))
                throw new ArgumentException("Id cannot be default", nameof(id));
        }

        // Virtual methods (خوبه، نگه دار)
        protected virtual void OnCreated() { }
        protected virtual void OnUpdated() { }
        protected virtual void OnDeleted() { }
    }
}