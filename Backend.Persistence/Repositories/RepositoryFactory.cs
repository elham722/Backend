using Backend.Domain.Aggregates.Common;
using Backend.Domain.Interfaces.Repositories;
using Backend.Persistence.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Persistence.Repositories;

/// <summary>
/// Factory for creating repositories
/// </summary>
public class RepositoryFactory : IRepositoryFactory
{
    private readonly ApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public RepositoryFactory(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public IReadRepository<T, TId> GetReadRepository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct
    {
        // Try to get from DI first
        var repository = _serviceProvider.GetService<IReadRepository<T, TId>>();
        if (repository != null)
        {
            return repository;
        }

        // Create a new instance if not registered
        return new GenericRepository<T, TId>(_context);
    }

    public IWriteRepository<T, TId> GetWriteRepository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct
    {
        // Try to get from DI first
        var repository = _serviceProvider.GetService<IWriteRepository<T, TId>>();
        if (repository != null)
        {
            return repository;
        }

        // Create a new instance if not registered
        return new GenericRepository<T, TId>(_context);
    }

    public IGenericRepository<T, TId> GetRepository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct
    {
        // Try to get from DI first
        var repository = _serviceProvider.GetService<IGenericRepository<T, TId>>();
        if (repository != null)
        {
            return repository;
        }

        // Create a new instance if not registered
        return new GenericRepository<T, TId>(_context);
    }

    public IGenericRepository<T, TId> CreateRepository<T, TId>() where T : BaseAggregateRoot<TId> where TId : struct
    {
        // Try to get from DI first
        var repository = _serviceProvider.GetService<IGenericRepository<T, TId>>();
        if (repository != null)
        {
            return repository;
        }

        // Create a new instance if not registered
        return new GenericRepository<T, TId>(_context);
    }

    public ICustomerRepository GetCustomerRepository()
    {
        // Try to get from DI first
        var repository = _serviceProvider.GetService<ICustomerRepository>();
        if (repository != null)
        {
            return repository;
        }

        // Create a new instance if not registered
        return new CustomerRepository(_context);
    }
} 