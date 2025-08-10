using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Interfaces.Repositories;

namespace Backend.Domain.Interfaces.UnitOfWork
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        // Repository access
        IGenericRepository<T, TId> Repository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct;
        
        // Specific repositories
        ICustomerRepository CustomerRepository { get; }
        
        // Transaction management
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        
        // Save changes
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> HasChangesAsync(CancellationToken cancellationToken = default);
        
        // Domain events
        Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default);
    }
} 