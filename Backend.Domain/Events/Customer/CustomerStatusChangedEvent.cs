using Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Events.Customer
{
    public class CustomerStatusChangedEvent : BaseDomainEvent
    {
        public Guid CustomerId { get; }
        public CustomerStatus NewStatus { get; }
        public CustomerStatus? PreviousStatus { get; }
        public string? Reason { get; }

        public CustomerStatusChangedEvent(Guid customerId, CustomerStatus newStatus, CustomerStatus? previousStatus = null, string? reason = null)
            : base(DateTime.UtcNow)
        {
            CustomerId = customerId;
            NewStatus = newStatus;
            PreviousStatus = previousStatus;
            Reason = reason;
        }
    }
}
