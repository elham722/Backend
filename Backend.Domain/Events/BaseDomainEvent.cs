using System;

namespace Backend.Domain.Events
{
    public abstract class BaseDomainEvent
    {
        public DateTime OccurredOn { get; }

        protected BaseDomainEvent(DateTime occurredOn)
        {
            OccurredOn = occurredOn;
        }
    }
} 