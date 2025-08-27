using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Entities;
using Backend.Domain.Entities.Common;
using Backend.Domain.Events;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;
using Backend.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Persistence.Contexts;

/// <summary>
/// Main application database context with auditing support
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly IDateTimeService? _dateTimeService;
    private readonly ILogger<ApplicationDbContext>? _logger;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        // Design-time constructor - no dependencies required
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTimeService dateTimeService,
        ILogger<ApplicationDbContext> logger) : base(options)
    {
        _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // DbSets
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable sensitive data logging in development
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());

        // Configure global query filters
        ConfigureGlobalQueryFilters(modelBuilder);

        // Ignore DomainEvents and AggregateEvents in all entities
        ConfigureDomainEventsIgnore(modelBuilder);
    }

    private void ConfigureDomainEventsIgnore(ModelBuilder modelBuilder)
    {
        // Ignore DomainEvents and AggregateEvents properties in all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity<Guid>).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Ignore("DomainEvents");
            }

            // Also ignore AggregateEvents for aggregate roots
            if (typeof(BaseAggregateRoot<Guid>).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Ignore("AggregateEvents");
            }
        }
    }

    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity<Guid>).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity<Guid>.IsDeleted));
                var value = Expression.Constant(false);
                var body = Expression.Equal(property, value);
                var lambda = Expression.Lambda(body, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply audit information before saving (only if services are available)
            if (_dateTimeService != null)
            {
                ApplyAuditInformation();
            }

            var result = await base.SaveChangesAsync(cancellationToken);
            
            _logger?.LogInformation("Saved {Count} changes to database", result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    private void ApplyAuditInformation()
    {
        if (_dateTimeService == null) return;

        var entries = ChangeTracker.Entries<BaseEntity<Guid>>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        var currentTime = _dateTimeService.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedAt(currentTime);
                entry.Entity.SetUpdatedAt(currentTime);
                
                // Note: CreatedBy and UpdatedBy should be set by the application layer
                // based on the current user context
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdatedAt(currentTime);
                
                // Note: UpdatedBy should be set by the application layer
                // based on the current user context
            }
        }
    }

    /// <summary>
    /// Sets the audit user information for the current operation
    /// </summary>
    public void SetAuditUser(string userId)
    {
        var entries = ChangeTracker.Entries<BaseEntity<Guid>>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedBy(userId);
                entry.Entity.SetUpdatedBy(userId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdatedBy(userId);
            }
        }
    }
} 