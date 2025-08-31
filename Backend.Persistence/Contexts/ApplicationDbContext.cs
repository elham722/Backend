using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Entities;
using Backend.Domain.Entities.Common;
using Backend.Domain.Entities.MFA;
using Backend.Domain.Events;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;
using Backend.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Persistence.Contexts;

/// <summary>
/// Main application database context
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Customer> Customers { get; set; }
    public DbSet<MfaMethod> MfaMethods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new MfaMethodConfiguration());

        // Configure global query filters and domain events ignore
        ConfigureGlobalSettings(modelBuilder);
    }

    private void ConfigureGlobalSettings(ModelBuilder modelBuilder)
    {
        // Ignore Domain Events in all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Ignore DomainEvents property in BaseEntity
            if (typeof(BaseEntity<Guid>).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Ignore("DomainEvents");
            }

            // Ignore AggregateEvents property in BaseAggregateRoot
            if (typeof(BaseAggregateRoot<Guid>).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Ignore("AggregateEvents");
            }
        }

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
} 