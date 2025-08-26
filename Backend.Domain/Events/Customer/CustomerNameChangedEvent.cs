using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Events.Customer
{
    public class CustomerNameChangedEvent : BaseDomainEvent
    {
        public Guid CustomerId { get; }
        public string NewFullName { get; }
        public string? PreviousFullName { get; }

        public CustomerNameChangedEvent(Guid customerId, string newFullName, string? previousFullName = null, DateTime? occurredOn = null)
            : base(occurredOn ?? DateTime.UtcNow)
        {
            CustomerId = customerId;
            NewFullName = newFullName ?? throw new ArgumentNullException(nameof(newFullName));
            PreviousFullName = previousFullName;
        }
    }
}
