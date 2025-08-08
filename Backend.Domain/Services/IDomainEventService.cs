using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Domain.Events;

namespace Backend.Domain.Services
{
    public interface IDomainEventService
    {
        Task PublishAsync(BaseDomainEvent domainEvent);
        Task PublishAsync(IEnumerable<BaseDomainEvent> domainEvents);
    }
} 