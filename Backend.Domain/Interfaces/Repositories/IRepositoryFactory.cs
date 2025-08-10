using Backend.Domain.Aggregates.Common;
using System;

namespace Backend.Domain.Interfaces.Repositories
{
    public interface IRepositoryFactory
    {
        IReadRepository<T, TId> GetReadRepository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct;
        IWriteRepository<T, TId> GetWriteRepository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct;
        IGenericRepository<T, TId> GetRepository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct;
        IGenericRepository<T, TId> CreateRepository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct;
        
        // Specific repositories
        ICustomerRepository GetCustomerRepository();
    }
} 