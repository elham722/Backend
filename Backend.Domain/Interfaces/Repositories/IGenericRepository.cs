using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Backend.Domain.Aggregates.Common;

namespace Backend.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T, TId> : IReadRepository<T, TId>, IWriteRepository<T, TId> 
        where T : BaseAggregateRoot<TId>
    {
        // Additional methods specific to generic repository can be added here
        // For example, methods that combine read and write operations
    }
} 