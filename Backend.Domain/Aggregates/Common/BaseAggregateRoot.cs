using System;
using System.Collections.Generic;
using Backend.Domain.Events;
using Backend.Domain.Entities.Common;

namespace Backend.Domain.Aggregates.Common
{
    public abstract class BaseAggregateRoot<TId> : BaseEntity<TId>
    {
        // Optimistic concurrency control
        public int Version { get; protected set; } = 0;

        // Aggregate-specific domain events
        private List<BaseDomainEvent>? _aggregateEvents;
        public IReadOnlyCollection<BaseDomainEvent> AggregateEvents => 
            _aggregateEvents?.AsReadOnly() ?? new List<BaseDomainEvent>().AsReadOnly();

        protected BaseAggregateRoot() : base() { }

        protected BaseAggregateRoot(TId id) : base(id) { }

        // Version management
        protected void IncrementVersion()
        {
            Version++;
        }

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
            _aggregateEvents.Add(@event);
            
            // Also add to base domain events
            AddDomainEvent(@event);
        }

        public void ClearAggregateEvents() => _aggregateEvents?.Clear();

        public bool HasAggregateEvents => _aggregateEvents?.Count > 0;

        // Aggregate state validation
        protected virtual void ValidateAggregateState()
        {
            // Override in derived classes for aggregate-specific validation
        }

        // Aggregate invariants
        protected virtual bool ValidateInvariants()
        {
            // Override in derived classes to validate aggregate invariants
            return true;
        }

        // Aggregate consistency
        protected void EnsureConsistency()
        {
            if (!ValidateInvariants())
                throw new InvalidOperationException("Aggregate invariants violated");

            ValidateAggregateState();
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
        public bool IsConsistent() => ValidateInvariants();

        public void EnsureValidState()
        {
            EnsureConsistency();
        }
    }
} 