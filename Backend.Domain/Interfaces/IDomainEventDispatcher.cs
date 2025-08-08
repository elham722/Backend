using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Events;

namespace Backend.Domain.Interfaces
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(BaseDomainEvent domainEvent, CancellationToken ct = default);
    }
}
