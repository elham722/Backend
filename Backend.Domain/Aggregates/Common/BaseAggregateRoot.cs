using System;
using System.Collections.Generic;
using Backend.Domain.Entities.Common;
using Backend.Domain.Events;

namespace Backend.Domain.Aggregates.Common 
{
    public abstract class BaseAggregateRoot<TId> : BaseEntity<TId>, IAggregateRoot where TId : IEquatable<TId>
    {
        // Optimistic concurrency control
        public int Version { get; protected set; }

        // Aggregate-specific domain events (optional, اگر جدا لازم داری)
        private List<BaseDomainEvent>? _aggregateEvents;
        public IReadOnlyCollection<BaseDomainEvent> AggregateEvents =>
            _aggregateEvents?.AsReadOnly() ?? new List<BaseDomainEvent>().AsReadOnly();

        protected BaseAggregateRoot() : base()
        {
            Version = 0; // Explicit initialization
        }

        protected BaseAggregateRoot(TId id) : base(id)
        {
            Version = 0;
        }

        // Version management
        protected void IncrementVersion() => Version++;

        protected void SetVersion(int version)
        {
            if (version < 0)
                throw new ArgumentException("Version cannot be negative", nameof(version));
            Version = version;
        }

        // Aggregate-specific domain event management
        protected void AddAggregateEvent(BaseDomainEvent @event)
        {
            _aggregateEvents ??= new List<BaseDomainEvent>();
            _aggregateEvents.Add(@event ?? throw new ArgumentNullException(nameof(@event)));
            AddDomainEvent(@event); // همچنان به BaseEntity می‌فرستیم
        }

        public void ClearAggregateEvents()
        {
            _aggregateEvents?.Clear();
            ClearDomainEvents(); // هماهنگی با BaseEntity
        }

        public bool HasAggregateEvents => (_aggregateEvents?.Count ?? 0) > 0;

        // Aggregate validation
        protected virtual void ValidateState()
        {
            // Override در subclasses برای validation خاص
        }

        protected void EnsureInvariants()
        {
            ValidateState();
            // Invariants خاص در subclasses تعریف می‌شه
        }

        // Override base methods to include version management
        protected override void OnCreated()
        {
            base.OnCreated();
            IncrementVersion();
        }

        protected override void OnUpdated()
        {
            base.OnUpdated();
            IncrementVersion();
        }

        protected override void OnDeleted()
        {
            base.OnDeleted();
            IncrementVersion();
        }

        // Aggregate-specific business logic
        public void EnsureValidState() => EnsureInvariants();
    }

    // Marker interface برای aggregates
    public interface IAggregateRoot { }
}