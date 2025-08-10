# Backend.Persistence

## Overview

The Persistence layer is responsible for data access and database operations. It implements the Repository pattern and Unit of Work pattern to provide a clean abstraction over the data access layer.

## Architecture

```
Backend.Persistence/
├── Contexts/                 # Database contexts
├── Configurations/           # Entity Framework configurations
├── Repositories/            # Repository implementations
├── UnitOfWork/              # Unit of Work implementation
├── DependencyInjection/     # Service registration
├── Migrations/              # Database migrations
└── Seeders/                 # Database seeders
```

## Key Components

### 1. ApplicationDbContext

The main database context that manages entity configurations and provides database access.

**Features:**
- Automatic audit field management (CreatedAt, UpdatedAt)
- Global query filters for soft delete
- Entity configurations
- Sensitive data logging in development

### 2. Entity Configurations

Entity Framework configurations for mapping domain entities to database tables.

**CustomerConfiguration:**
- Table structure and constraints
- Value object mappings (Email, PhoneNumber, Address)
- Indexes for performance optimization
- Audit field configurations

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
- `DeleteAsync()` - Delete entity by ID

#### CustomerRepository

Specific repository for Customer entity with business-specific queries:

**Customer-Specific Methods:**
- `GetByEmailAsync()` - Get customer by email
- `GetByPhoneNumberAsync()` - Get customer by phone number
- `GetPremiumCustomersAsync()` - Get premium customers
- `GetVerifiedCustomersAsync()` - Get verified customers
- `GetCustomersByStatusAsync()` - Get customers by status
- `GetCustomersByAgeRangeAsync()` - Get customers by age range
- `GetCustomersByLocationAsync()` - Get customers by location
- `EmailExistsAsync()` - Check if email exists
- `PhoneNumberExistsAsync()` - Check if phone number exists
- `GetCustomerCountByStatusAsync()` - Get customer count by status
- `GetPremiumCustomerCountAsync()` - Get premium customer count
- `GetVerifiedCustomerCountAsync()` - Get verified customer count
- `GetCustomersCreatedInDateRangeAsync()` - Get customers by creation date range
- `GetCustomersWithIncompleteProfileAsync()` - Get customers with incomplete profiles
- `GetCustomersBySearchTermAsync()` - Search customers by term

### 4. Unit of Work

Manages database transactions and coordinates multiple repositories.

**Features:**
- Transaction management (Begin, Commit, Rollback)
- Repository coordination
- Automatic disposal
- Async/await support

**Methods:**
- `BeginTransactionAsync()` - Start a new transaction
- `CommitTransactionAsync()` - Commit the current transaction
- `RollbackTransactionAsync()` - Rollback the current transaction
- `SaveChangesAsync()` - Save all changes
- `SaveEntitiesAsync()` - Save entities and return success status

### 5. Dependency Injection

**PersistenceServicesRegistration:**

Provides extension methods for registering persistence services:

```csharp
// Using configuration
services.AddPersistenceServices(configuration);

// Using connection string directly
services.AddPersistenceServices(connectionString, enableSensitiveDataLogging);
```

**Registered Services:**
- `ApplicationDbContext` - Scoped
- `ICustomerRepository` - Scoped
- `IUnitOfWork` - Scoped

### 6. Database Migrations

**InitialCreate Migration:**
- Creates Customers table
- Configures all columns and constraints
- Creates indexes for performance
- Sets up value object mappings

### 7. Database Seeder

**DatabaseSeeder:**

Seeds the database with initial test data:

**Sample Data:**
- Premium customers
- Regular customers
- Business customers
- Young customers
- International customers
- Inactive customers

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

## Performance Considerations

1. **Indexes on frequently queried columns**
2. **Composite indexes for common query patterns**
3. **Eager loading with Include() for related data**
4. **Pagination for large result sets**
5. **Connection string retry policies**
6. **Query optimization with proper predicates**
7. **Use of IQueryable for deferred execution**

## Security

1. **Parameterized queries (automatic with EF Core)**
2. **Connection string security**
3. **Audit trail with CreatedBy/UpdatedBy**
4. **Soft delete for data retention**
5. **Input validation at repository level**
6. **Sensitive data logging only in development** 