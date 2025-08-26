using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Interfaces.Repositories;

namespace Backend.Domain.Interfaces.UnitOfWork
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IGenericRepository<T, TId> Repository<T, TId>() where T : BaseAggregateRoot<TId> where TId : IEquatable<TId>;
        ICustomerRepository CustomerRepository { get; }
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> HasChangesAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Dispatches all domain events from aggregates after saving changes.
        /// </summary>
        Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default);
    }
} 