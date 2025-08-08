using System;

namespace Backend.Domain.Events
{
    public abstract class BaseDomainEvent 
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
} 