using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Backend.Persistence.Contexts;

/// <summary>
/// Simple ApplicationDbContext for testing migrations without ValueObjects
/// </summary>
public class SimpleApplicationDbContext : DbContext
{
    public SimpleApplicationDbContext(DbContextOptions<SimpleApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Simple Customer table without ValueObjects
        modelBuilder.Entity<SimpleCustomer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.CustomerStatus).IsRequired().HasConversion<string>();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }
}

/// <summary>
/// Simple Customer entity without ValueObjects for testing
/// </summary>
public class SimpleCustomer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string CustomerStatus { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Design-time factory for SimpleApplicationDbContext
/// </summary>
public class SimpleApplicationDbContextFactory : IDesignTimeDbContextFactory<SimpleApplicationDbContext>
{
    public SimpleApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<SimpleApplicationDbContext>();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=BackendDb;Trusted_Connection=true;MultipleActiveResultSets=true";
        }

        optionsBuilder.UseSqlServer(connectionString);

        return new SimpleApplicationDbContext(optionsBuilder.Options);
    }
} 