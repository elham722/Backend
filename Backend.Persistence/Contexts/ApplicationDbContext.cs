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

  

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable sensitive data logging in development
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

  
} 