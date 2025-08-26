using Backend.Domain.Aggregates.Common;
using System;

namespace Backend.Domain.Interfaces.Repositories
{
    public interface IRepositoryFactory
    {
        public interface IRepositoryFactory
        {
            IGenericRepository<T, TId> GetRepository<T, TId>() where T : BaseAggregateRoot<TId> where TId : IEquatable<TId>;
            ICustomerRepository GetCustomerRepository();
        }
    }
} 