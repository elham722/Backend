using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.UnitOfWork;
using Backend.Persistence.Contexts;
using Backend.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Backend.Persistence.UnitOfWork;

/// <summary>
/// Unit of Work implementation for managing database transactions and repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly Dictionary<Type, object> _repositories;
    private IDbContextTransaction? _currentTransaction;

    // Repository properties
    public ICustomerRepository CustomerRepository { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        ILogger<UnitOfWork> logger,
        ICustomerRepository customerRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CustomerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        
        _repositories = new Dictionary<Type, object>();
    }

    #region Repository Management

    /// <summary>
    /// Gets a generic repository for the specified entity type
    /// </summary>
    public IGenericRepository<T, TId> Repository<T, TId>() where T : BaseAggregateRoot<TId> where TId : IEquatable<TId>
    {
        var entityType = typeof(T);
        
        if (_repositories.ContainsKey(entityType))
        {
            return (IGenericRepository<T, TId>)_repositories[entityType];
        }

        var repositoryType = typeof(BaseRepository<T, TId>);
        var repository = Activator.CreateInstance(repositoryType, _context, _logger);
        
        if (repository == null)
        {
            throw new InvalidOperationException($"Failed to create repository for type {entityType.Name}");
        }

        _repositories[entityType] = repository;
        return (IGenericRepository<T, TId>)repository;
    }

    #endregion

    #region Transaction Management

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                _logger.LogWarning("Transaction already exists. Creating nested transaction.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            _logger.LogInformation("Database transaction started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error beginning transaction");
            throw;
        }
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction == null)
            {
                _logger.LogWarning("No active transaction to commit");
                return;
            }

            await _currentTransaction.CommitAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
            
            _logger.LogInformation("Database transaction committed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Rollbacks the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction == null)
            {
                _logger.LogWarning("No active transaction to rollback");
                return;
            }

            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
            
            _logger.LogInformation("Database transaction rolled back");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction");
            throw;
        }
    }

    #endregion

    #region Change Tracking

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Saved {Count} changes to database", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    /// <summary>
    /// Checks if there are any unsaved changes
    /// </summary>
    public async Task<bool> HasChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Task.FromResult(_context.ChangeTracker.HasChanges());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for changes");
            throw;
        }
    }

    #endregion

    #region Domain Events

    /// <summary>
    /// Dispatches all domain events from aggregates after saving changes
    /// </summary>
    public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all entities that have domain events
            var entitiesWithEvents = _context.ChangeTracker.Entries<BaseAggregateRoot<Guid>>()
                .Where(e => e.Entity.HasDomainEvents)
                .Select(e => e.Entity)
                .ToList();

            // Extract domain events
            var domainEvents = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear domain events from entities
            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            // Dispatch domain events
            foreach (var domainEvent in domainEvents)
            {
                _logger.LogInformation("Dispatching domain event: {EventType}", domainEvent.GetType().Name);
                
                // Note: In a real implementation, you would use a domain event dispatcher
                // For now, we'll just log the event
                await Task.CompletedTask;
            }

            _logger.LogInformation("Dispatched {Count} domain events", domainEvents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching domain events");
            throw;
        }
    }

    #endregion

    #region Disposal

    /// <summary>
    /// Disposes the unit of work
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the unit of work asynchronously
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentTransaction?.Dispose();
            _context?.Dispose();
        }
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
        }
    }

    #endregion
} 