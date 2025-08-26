# Backend.Persistence

## Overview

The Persistence layer is responsible for data access and database operations. It implements the Repository pattern and Unit of Work pattern to provide a clean abstraction over the data access layer. The layer is designed to work with both Domain entities and Identity users through a bridge service.

## Architecture

```
Backend.Persistence/
├── Contexts/                 # Database contexts
├── Configurations/           # Entity Framework configurations
├── Repositories/            # Repository implementations
├── Services/                # Bridge services for Identity integration
├── UnitOfWork/              # Unit of Work implementation
├── DependencyInjection/     # Service registration
├── Migrations/              # Database migrations
└── Seeders/                 # Database seeders
```

## Key Components

### 1. ApplicationDbContext

The main database context that manages entity configurations and provides database access.

**Features:**
- Automatic audit field management (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Global query filters for soft delete
- Entity configurations
- Value object conversions
- Sensitive data logging in development

**Audit Support:**
```csharp
// Automatic audit field population
_context.SetAuditUser(currentUserId);

// Manual audit field setting
public void SetAuditUser(string userId)
{
    var entries = ChangeTracker.Entries<BaseEntity>()
        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
    
    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.CreatedBy = userId;
            entry.Entity.UpdatedBy = userId;
        }
        else if (entry.State == EntityState.Modified)
        {
            entry.Entity.UpdatedBy = userId;
        }
    }
}
```

### 2. Entity Configurations

Entity Framework configurations for mapping domain entities to database tables.

**CustomerConfiguration:**
- Table structure and constraints
- Value object mappings (Email, PhoneNumber, Address)
- Indexes for performance optimization
- Audit field configurations

**Value Object Conversions:**
```csharp
// Email value object conversion
modelBuilder.Entity<Customer>()
    .Property(c => c.Email)
    .HasConversion(
        email => email.Value,
        value => Email.Create(value));

// Address value object as owned entity
modelBuilder.Entity<Customer>()
    .OwnsOne(c => c.Address, address =>
    {
        address.Property(a => a.Street).HasMaxLength(200);
        address.Property(a => a.City).HasMaxLength(100);
        // ... other properties
    });
```

### 3. Repository Pattern

#### BaseRepository<T, TId>

Generic repository implementation providing common CRUD operations:

**Read Operations:**
- `GetByIdAsync()` - Get entity by ID
- `GetAllAsync()` - Get all entities
- `FindAsync()` - Find entities by predicate
- `FirstOrDefaultAsync()` - Get first entity matching predicate
- `ExistsAsync()` - Check if entity exists
- `CountAsync()` - Count entities
- `GetPagedAsync()` - Get paginated results
- `GetOrderedAsync()` - Get ordered results
- `GetQueryable()` - Get IQueryable for complex queries

**Write Operations:**
- `AddAsync()` - Add new entity
- `AddRangeAsync()` - Add multiple entities
- `Update()` - Update entity
- `UpdateRange()` - Update multiple entities
- `Delete()` - Delete entity
- `DeleteRange()` - Delete multiple entities
- `BulkInsertAsync()` - Bulk insert operations
- `BulkUpdateAsync()` - Bulk update operations
- `BulkDeleteAsync()` - Bulk delete operations

#### CustomerRepository

Specific repository for Customer entity with business-specific queries:

**Customer-Specific Methods:**
- `GetByApplicationUserIdAsync()` - Get customer by Identity user ID
- `GetByEmailAsync()` - Get customer by email
- `GetByPhoneAsync()` - Get customer by phone number
- `GetByStatusAsync()` - Get customers by status
- `GetActiveCustomersAsync()` - Get active customers
- `GetCustomersByRegistrationDateAsync()` - Get customers by registration date range
- `IsEmailUniqueAsync()` - Check if email is unique
- `IsPhoneUniqueAsync()` - Check if phone is unique
- `GetCustomerCountByStatusAsync()` - Get customer count by status

**Additional Business Methods:**
- `GetCustomersByAgeRangeAsync()` - Get customers by age range
- `GetCustomersByLocationAsync()` - Get customers by location
- `GetPremiumCustomersAsync()` - Get premium customers
- `GetVerifiedCustomersAsync()` - Get verified customers
- `GetCustomersWithIncompleteProfileAsync()` - Get customers with incomplete profiles
- `GetCustomersBySearchTermAsync()` - Search customers by term
- `GetPremiumCustomerCountAsync()` - Get premium customer count
- `GetVerifiedCustomerCountAsync()` - Get verified customer count

### 4. Unit of Work

Manages database transactions and coordinates multiple repositories.

**Features:**
- Transaction management (Begin, Commit, Rollback)
- Repository coordination
- Automatic disposal
- Async/await support
- Domain event dispatching

**Methods:**
- `BeginTransactionAsync()` - Start a new transaction
- `CommitTransactionAsync()` - Commit the current transaction
- `RollbackTransactionAsync()` - Rollback the current transaction
- `SaveChangesAsync()` - Save all changes
- `HasChangesAsync()` - Check if there are unsaved changes
- `DispatchDomainEventsAsync()` - Dispatch domain events

### 5. User Persistence Service

Bridge service that connects Persistence layer with Identity layer for user management.

**Features:**
- Creates users with both Domain entity and Identity user
- Updates users across both layers
- Manages transactions between Identity and Domain
- Provides unified user management interface

**Key Methods:**
- `CreateUserAsync()` - Create user with Domain and Identity
- `UpdateUserAsync()` - Update user across both layers
- `GetUserByIdAsync()` - Get user with both Domain and Identity info
- `GetUsersAsync()` - Get paginated users with filtering
- `DeleteUserAsync()` - Delete user (soft or permanent)
- `ActivateUserAsync()` - Activate user account
- `DeactivateUserAsync()` - Deactivate user account

**Transaction Management:**
```csharp
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
```

### 6. Dependency Injection

**PersistenceServicesRegistration:**

Provides extension methods for registering persistence services:

```csharp
// Using configuration
services.AddPersistenceServices(configuration);

// Using connection string directly
services.AddPersistenceServices(connectionString, enableSensitiveDataLogging);

// For testing
services.AddPersistenceServicesForTesting(databaseName);
```

**Registered Services:**
- `ApplicationDbContext` - Scoped
- `ICustomerRepository` - Scoped
- `IUnitOfWork` - Scoped
- `IRepositoryFactory` - Scoped
- `IUserPersistenceService` - Scoped

## Database Schema

### Customers Table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | uniqueidentifier | PK, Identity | Primary key |
| FirstName | nvarchar(100) | NOT NULL | Customer first name |
| LastName | nvarchar(100) | NOT NULL | Customer last name |
| DateOfBirth | datetime2 | NOT NULL | Customer date of birth |
| CustomerStatus | nvarchar(max) | NOT NULL | Customer status (Active, Inactive, Suspended) |
| IsVerified | bit | NOT NULL, Default: false | Verification status |
| IsPremium | bit | NOT NULL, Default: false | Premium status |
| Email | nvarchar(255) | NULL, Unique | Customer email |
| PhoneNumber | nvarchar(20) | NULL, Unique | Customer phone number |
| ApplicationUserId | nvarchar(450) | NULL, FK | Link to Identity user |
| Street | nvarchar(200) | NULL | Address street |
| City | nvarchar(100) | NULL | Address city |
| State | nvarchar(100) | NULL | Address state |
| Country | nvarchar(100) | NULL | Address country |
| PostalCode | nvarchar(20) | NULL | Address postal code |
| Province | nvarchar(100) | NULL | Address province |
| CreatedAt | datetime2 | NOT NULL | Creation timestamp |
| UpdatedAt | datetime2 | NOT NULL | Last update timestamp |
| CreatedBy | nvarchar(100) | NULL | Creator identifier |
| UpdatedBy | nvarchar(100) | NULL | Last updater identifier |
| EntityStatus | nvarchar(max) | NOT NULL, Default: Active | Entity status |

### Indexes

**Single Column Indexes:**
- `IX_Customers_CreatedAt`
- `IX_Customers_CustomerStatus`
- `IX_Customers_DateOfBirth`
- `IX_Customers_Email` (Unique)
- `IX_Customers_FirstName`
- `IX_Customers_IsPremium`
- `IX_Customers_IsVerified`
- `IX_Customers_LastName`
- `IX_Customers_PhoneNumber` (Unique)
- `IX_Customers_ApplicationUserId` (Unique)

**Composite Indexes:**
- `IX_Customers_FirstName_LastName`
- `IX_Customers_CustomerStatus_IsVerified`
- `IX_Customers_IsPremium_CustomerStatus`

## Usage Examples

### Basic Repository Usage

```csharp
// Get customer by ID
var customer = await _customerRepository.GetByIdAsync(customerId);

// Get all customers
var customers = await _customerRepository.GetAllAsync();

// Find customers by predicate
var activeCustomers = await _customerRepository.FindAsync(c => c.CustomerStatus == CustomerStatus.Active);

// Get paginated results
var (customers, totalCount) = await _customerRepository.GetPagedAsync(1, 10);
```

### Unit of Work Usage

```csharp
// Using transaction
await _unitOfWork.BeginTransactionAsync();

try
{
    var customer = Customer.Create(...);
    await _unitOfWork.CustomerRepository.AddAsync(customer);
    
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

### User Management with Bridge Service

```csharp
// Create user with both Domain and Identity
var result = await _userPersistenceService.CreateUserAsync(createUserDto, currentUserId);

// Update user across both layers
var updateResult = await _userPersistenceService.UpdateUserAsync(userId, updateUserDto, currentUserId);

// Get user with both Domain and Identity information
var userResult = await _userPersistenceService.GetUserByIdAsync(userId, includeRoles: true);

// Get paginated users with filtering
var usersResult = await _userPersistenceService.GetUsersAsync(
    pageNumber: 1,
    pageSize: 10,
    searchTerm: "john",
    status: "Active",
    role: "Customer");
```

### Complex Queries

```csharp
// Get queryable for complex queries
var query = _customerRepository.GetQueryable();

var result = await query
    .Where(c => c.IsPremium && c.IsVerified)
    .OrderByDescending(c => c.CreatedAt)
    .Skip(0)
    .Take(10)
    .ToListAsync();
```

## Configuration

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BackendDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Development Settings

```json
{
  "EnableSensitiveDataLogging": true
}
```

## Integration with Identity Layer

The Persistence layer integrates with the Identity layer through the `UserPersistenceService`:

1. **Separation of Concerns**: Identity layer manages authentication/authorization, Persistence layer manages Domain entities
2. **Bridge Service**: `UserPersistenceService` coordinates operations between both layers
3. **Transaction Management**: Ensures consistency across Identity and Domain operations
4. **Audit Trail**: Maintains audit information in both layers
5. **Value Objects**: Properly maps Domain value objects to database columns

### Identity Integration Flow

```
Application Layer
       ↓
UserPersistenceService (Bridge)
       ↓
┌─────────────────┬─────────────────┐
│   Identity      │    Domain       │
│   Layer         │    Layer        │
│                 │                 │
│ ApplicationUser │    Customer     │
│ UserManager     │   Repository    │
│ RoleManager     │   UnitOfWork    │
└─────────────────┴─────────────────┘
```

## Best Practices

1. **Always use Unit of Work for transactions**
2. **Use repository interfaces in application layer**
3. **Leverage Entity Framework configurations for complex mappings**
4. **Use indexes for frequently queried columns**
5. **Implement soft delete with global query filters**
6. **Use value objects for complex data types**
7. **Enable sensitive data logging only in development**
8. **Use async/await for all database operations**
9. **Implement proper disposal patterns**
10. **Use connection string retry policies**
11. **Maintain separation between Identity and Domain layers**
12. **Use bridge services for cross-layer operations**

## Performance Considerations

1. **Indexes on frequently queried columns**
2. **Composite indexes for common query patterns**
3. **Eager loading with Include() for related data**
4. **Pagination for large result sets**
5. **Connection string retry policies**
6. **Query optimization with proper predicates**
7. **Use of IQueryable for deferred execution**
8. **AsNoTracking() for read-only operations**

## Security

1. **Parameterized queries (automatic with EF Core)**
2. **Connection string security**
3. **Audit trail with CreatedBy/UpdatedBy**
4. **Soft delete for data retention**
5. **Input validation at repository level**
6. **Sensitive data logging only in development**
7. **Proper transaction isolation levels**
8. **Role-based access control through Identity integration** 