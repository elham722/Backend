using Backend.Domain.Aggregates.Common;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.UnitOfWork;
using Backend.Persistence.Contexts;
using Backend.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work implementation for managing database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IRepositoryFactory _repositoryFactory;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    // Repositories
    private ICustomerRepository? _customerRepository;

    public UnitOfWork(ApplicationDbContext context, IRepositoryFactory repositoryFactory)
    {
        _context = context;
        _repositoryFactory = repositoryFactory;
    }

    #region Repositories

    public IGenericRepository<T, TId> Repository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct
    {
        return _repositoryFactory.CreateRepository<T, TId>();
    }

    public ICustomerRepository CustomerRepository => 
        _customerRepository ??= new CustomerRepository(_context);

    #endregion

    #region Transaction Management

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            // Log the rollback error but don't throw
            // This prevents masking the original exception
        }
    }

    #endregion

    #region Save Changes

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_context.ChangeTracker.HasChanges());
    }

    #endregion

    #region Domain Events

    public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        // This will be implemented when we add domain event dispatcher
        // For now, just complete the task
        await Task.CompletedTask;
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }
            await _context.DisposeAsync();
        }
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion
} 