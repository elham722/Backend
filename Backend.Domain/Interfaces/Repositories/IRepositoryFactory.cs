using Backend.Domain.Aggregates.Common;
using System;

namespace Backend.Domain.Interfaces.Repositories
{
    public interface IRepositoryFactory
    {
        IReadRepository<T, TId> GetReadRepository<T, TId>() where T : BaseAggregateRoot<TId>;
        IWriteRepository<T, TId> GetWriteRepository<T, TId>() where T : BaseAggregateRoot<TId>;
        IGenericRepository<T, TId> GetRepository<T, TId>() where T : BaseAggregateRoot<TId>;
        
        // Specific repositories
        ICustomerRepository GetCustomerRepository();
    }
} 