using System;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Interfaces.UnitOfWork;
using Backend.Persistence.Contexts;
using Backend.Persistence.Repositories;
using Backend.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Persistence.DependencyInjection;

/// <summary>
/// Dependency injection registration for Persistence layer
/// </summary>
public static class PersistenceServicesRegistration
{
    /// <summary>
    /// Registers all Persistence services using configuration
    /// </summary>
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DBConnection");
        var enableSensitiveDataLogging = configuration.GetValue<bool>("EnableSensitiveDataLogging", false);

        return services.AddPersistenceServices(connectionString, enableSensitiveDataLogging);
    }

    /// <summary>
    /// Registers all Persistence services using connection string directly
    /// </summary>
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        string connectionString,
        bool enableSensitiveDataLogging = false)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
        }

        // Register DbContext
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            if (enableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Register Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // Register Repository Factory (if needed)
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();

   

        return services;
    }

    /// <summary>
    /// Registers Persistence services for testing
    /// </summary>
    public static IServiceCollection AddPersistenceServicesForTesting(
        this IServiceCollection services,
        string databaseName = "TestDatabase")
    {
        // Use in-memory database for testing
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.EnableSensitiveDataLogging();
        });

        // Register Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // Register Repository Factory
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();

      

        return services;
    }
}

/// <summary>
/// Repository factory implementation
/// </summary>
public class RepositoryFactory : IRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;

    public RepositoryFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IGenericRepository<T, TId> CreateRepository<T, TId>() 
        where T : Backend.Domain.Aggregates.Common.BaseAggregateRoot<TId> 
        where TId : IEquatable<TId>
    {
        return _serviceProvider.GetRequiredService<IGenericRepository<T, TId>>();
    }

    public ICustomerRepository CreateCustomerRepository()
    {
        return _serviceProvider.GetRequiredService<ICustomerRepository>();
    }
} 