# Persistence Layer Implementation Summary

## Overview

This document summarizes the implementation of the Persistence layer for the User Management module, following Clean Architecture principles and maintaining separation between Identity and Domain layers.

## Architecture Decisions

### 1. Layer Separation
- **Identity Layer**: Handles authentication, authorization, and user identity management
- **Persistence Layer**: Manages Domain entities and data access
- **Bridge Service**: Coordinates operations between Identity and Domain layers

### 2. Repository Pattern
- Generic `BaseRepository<T, TId>` for common CRUD operations
- Specific `CustomerRepository` for business-specific queries
- Interface-based design for testability and dependency inversion

### 3. Unit of Work Pattern
- Manages database transactions
- Coordinates multiple repositories
- Ensures data consistency across operations

### 4. Value Object Support
- Proper mapping of Domain value objects to database columns
- Type-safe conversions for Email, PhoneNumber, and Address
- Owned entity configuration for complex value objects

## Key Components

### 1. ApplicationDbContext

**Enhanced with:**
- Automatic audit field management (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Global query filters for soft delete
- Value object conversions
- Transaction support

```csharp
public class ApplicationDbContext : DbContext
{
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<ApplicationDbContext> _logger;

    // Automatic audit field population
    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = _dateTimeService.Now;
                entry.Entity.UpdatedAt = _dateTimeService.Now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = _dateTimeService.Now;
            }
        }
    }

    // Manual audit user setting
    public void SetAuditUser(string userId)
    {
        // Sets CreatedBy and UpdatedBy fields
    }
}
```

### 2. BaseRepository<T, TId>

**Features:**
- Full CRUD operations with async support
- Pagination and filtering
- Bulk operations
- Queryable support for complex queries
- Comprehensive error handling and logging

```csharp
public abstract class BaseRepository<T, TId> : IGenericRepository<T, TId>
    where T : BaseAggregateRoot<TId>
    where TId : IEquatable<TId>
{
    // Read operations with AsNoTracking for performance
    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }

    // Pagination support
    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.AsNoTracking().CountAsync(cancellationToken);
        var items = await _dbSet
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (items, totalCount);
    }
}
```

### 3. CustomerRepository

**Business-Specific Methods:**
- Identity integration: `GetByApplicationUserIdAsync()`
- Email and phone uniqueness checks
- Status-based queries
- Advanced filtering and search

```csharp
public class CustomerRepository : BaseRepository<Customer, Guid>, ICustomerRepository
{
    public async Task<Customer?> GetByApplicationUserIdAsync(string applicationUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return !await _dbSet
            .AsNoTracking()
            .AnyAsync(c => c.Email.Value == email, cancellationToken);
    }
}
```

### 4. UnitOfWork

**Transaction Management:**
- Begin, commit, and rollback transactions
- Repository coordination
- Domain event dispatching
- Proper disposal patterns

```csharp
public class UnitOfWork : IUnitOfWork
{
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        // Extract and dispatch domain events from aggregates
        var entitiesWithEvents = _context.ChangeTracker.Entries<BaseAggregateRoot<Guid>>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
        
        // Process domain events...
    }
}
```

### 5. UserPersistenceService (Bridge Service)

**Key Features:**
- Coordinates operations between Identity and Domain layers
- Manages transactions across both contexts
- Provides unified user management interface
- Ensures data consistency

```csharp
public class UserPersistenceService : IUserPersistenceService
{
    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto, string createdBy, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Create Identity User
            var identityUser = ApplicationUser.Create(...);
            await _userManager.CreateAsync(identityUser, createUserDto.Password);
            
            // 2. Create Domain Customer entity
            var customer = Customer.Create(...);
            await _customerRepository.AddAsync(customer, cancellationToken);
            
            // 3. Link Identity and Domain
            identityUser.SetCustomerId(customer.Id);
            await _userManager.UpdateAsync(identityUser);
            
            // 4. Commit transaction
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            return Result<UserDto>.Success(userDto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
```

## Value Object Configuration

### Email Value Object
```csharp
modelBuilder.Entity<Customer>()
    .Property(c => c.Email)
    .HasConversion(
        email => email.Value,
        value => Email.Create(value));
```

### Address Value Object (Owned Entity)
```csharp
modelBuilder.Entity<Customer>()
    .OwnsOne(c => c.Address, address =>
    {
        address.Property(a => a.Street).HasMaxLength(200);
        address.Property(a => a.City).HasMaxLength(100);
        address.Property(a => a.State).HasMaxLength(100);
        address.Property(a => a.Country).HasMaxLength(100);
        address.Property(a => a.PostalCode).HasMaxLength(20);
        address.Property(a => a.Province).HasMaxLength(100);
    });
```

## Database Schema

### Customers Table
```sql
CREATE TABLE Customers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATETIME2 NOT NULL,
    CustomerStatus NVARCHAR(MAX) NOT NULL,
    IsVerified BIT NOT NULL DEFAULT 0,
    IsPremium BIT NOT NULL DEFAULT 0,
    Email NVARCHAR(255) NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NULL UNIQUE,
    ApplicationUserId NVARCHAR(450) NULL UNIQUE,
    Street NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    Country NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    Province NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NULL,
    UpdatedBy NVARCHAR(100) NULL,
    EntityStatus NVARCHAR(MAX) NOT NULL DEFAULT 'Active'
);
```

### Indexes
```sql
-- Single column indexes
CREATE INDEX IX_Customers_CreatedAt ON Customers(CreatedAt);
CREATE INDEX IX_Customers_CustomerStatus ON Customers(CustomerStatus);
CREATE INDEX IX_Customers_Email ON Customers(Email);
CREATE INDEX IX_Customers_PhoneNumber ON Customers(PhoneNumber);
CREATE INDEX IX_Customers_ApplicationUserId ON Customers(ApplicationUserId);

-- Composite indexes
CREATE INDEX IX_Customers_FirstName_LastName ON Customers(FirstName, LastName);
CREATE INDEX IX_Customers_CustomerStatus_IsVerified ON Customers(CustomerStatus, IsVerified);
```

## Dependency Injection

### Service Registration
```csharp
public static class PersistenceServicesRegistration
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        string connectionString,
        bool enableSensitiveDataLogging = false)
    {
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
        });

        // Register Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Bridge Service
        services.AddScoped<IUserPersistenceService, UserPersistenceService>();

        return services;
    }
}
```

## Integration Flow

### User Creation Flow
```
1. Application Layer calls UserPersistenceService.CreateUserAsync()
2. Bridge Service begins transaction
3. Creates Identity User via UserManager
4. Creates Domain Customer entity via Repository
5. Links Identity and Domain entities
6. Commits transaction
7. Returns unified UserDto
```

### User Update Flow
```
1. Application Layer calls UserPersistenceService.UpdateUserAsync()
2. Bridge Service begins transaction
3. Updates Identity User properties
4. Updates Domain Customer entity
5. Commits transaction
6. Returns updated UserDto
```

## Best Practices Implemented

### 1. Performance
- **AsNoTracking()** for read-only operations
- **Proper indexing** for frequently queried columns
- **Pagination** for large result sets
- **Eager loading** with Include() for related data

### 2. Security
- **Parameterized queries** (automatic with EF Core)
- **Audit trail** with CreatedBy/UpdatedBy
- **Soft delete** for data retention
- **Transaction isolation** for data consistency

### 3. Maintainability
- **Interface-based design** for testability
- **Separation of concerns** between Identity and Domain
- **Comprehensive error handling** and logging
- **Clean dependency injection** setup

### 4. Scalability
- **Repository pattern** for data access abstraction
- **Unit of Work** for transaction management
- **Bulk operations** support
- **Queryable support** for complex queries

## Testing Support

### In-Memory Database
```csharp
public static IServiceCollection AddPersistenceServicesForTesting(
    this IServiceCollection services,
    string databaseName = "TestDatabase")
{
    services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    {
        options.UseInMemoryDatabase(databaseName);
        options.EnableSensitiveDataLogging();
    });

    // Register all services...
    return services;
}
```

## Error Handling

### Repository Level
```csharp
public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
{
    try
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting entity by ID {Id}", id);
        throw;
    }
}
```

### Bridge Service Level
```csharp
public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto, string createdBy, CancellationToken cancellationToken = default)
{
    try
    {
        // Implementation...
        return Result<UserDto>.Success(userDto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating user");
        return Result<UserDto>.Failure(ex.Message, "CreateUserError");
    }
}
```

## Future Enhancements

### 1. Caching
- Implement caching for frequently accessed data
- Use Redis or in-memory caching
- Cache invalidation strategies

### 2. Event Sourcing
- Implement event sourcing for audit trails
- Store domain events for replay
- Event versioning and migration

### 3. Read Models
- Implement CQRS with separate read models
- Optimize queries for specific use cases
- Denormalized views for complex queries

### 4. Multi-tenancy
- Support for multi-tenant applications
- Tenant isolation strategies
- Shared database with tenant filtering

## Conclusion

The Persistence layer implementation provides a robust, scalable, and maintainable foundation for the User Management module. It successfully maintains separation between Identity and Domain layers while providing a unified interface through the bridge service. The implementation follows Clean Architecture principles and incorporates best practices for performance, security, and maintainability.

Key achievements:
- ✅ Clean separation between Identity and Domain layers
- ✅ Comprehensive repository pattern implementation
- ✅ Robust transaction management
- ✅ Proper value object mapping
- ✅ Audit trail support
- ✅ Performance optimizations
- ✅ Comprehensive error handling
- ✅ Testable architecture
- ✅ Scalable design 