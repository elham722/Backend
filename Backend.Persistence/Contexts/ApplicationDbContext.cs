using Backend.Domain.Entities;
using Backend.Domain.Entities.Common;
using Backend.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());

        // Global query filters
        ApplyGlobalQueryFilters(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable sensitive data logging in development
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    /// <summary>
    /// Applies global query filters for soft delete and audit
    /// </summary>
    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Soft delete filter for BaseEntity
        modelBuilder.Entity<Customer>()
            .HasQueryFilter(e => e.Status != Domain.Enums.EntityStatus.Deleted);
    }

    /// <summary>
    /// Override SaveChanges to automatically set audit fields
    /// </summary>
    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically set audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Sets audit fields (CreatedAt, UpdatedAt) automatically
    /// </summary>
    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity<Guid>>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedAt(DateTime.UtcNow);
                entry.Entity.SetUpdatedAt(DateTime.UtcNow);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdatedAt(DateTime.UtcNow);
            }
        }
    }
} 