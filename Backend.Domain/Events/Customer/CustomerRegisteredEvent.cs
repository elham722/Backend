using System;

namespace Backend.Domain.Events.Customer
{
    public class CustomerRegisteredEvent : BaseDomainEvent
    {
        public Guid CustomerId { get; }
        public string? ApplicationUserId { get; }

        public CustomerRegisteredEvent(Guid customerId, string? applicationUserId) : base(DateTime.UtcNow)
        {
            CustomerId = customerId;
            ApplicationUserId = applicationUserId;
        }


    }
} 