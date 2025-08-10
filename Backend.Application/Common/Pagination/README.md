# Pagination System Documentation

## Overview

This document describes the comprehensive pagination system implemented in the Backend.Application layer. The system provides flexible, reusable, and efficient pagination capabilities for all data queries.

## Components

### 1. PaginationDto
Base DTO for pagination parameters with the following features:
- **PageNumber**: Current page (1-based)
- **PageSize**: Items per page (max 100)
- **SearchTerm**: Global search across multiple fields
- **SortBy**: Field to sort by
- **SortDirection**: Sort direction (asc/desc)
- **Filters**: Additional key-value filters
- **Include**: Related entities to include
- **Select**: Specific fields to select

### 2. PaginatedResult<T>
Result wrapper for paginated data:
- **Data**: The actual data items
- **TotalCount**: Total number of items
- **PageNumber**: Current page number
- **PageSize**: Items per page
- **TotalPages**: Total number of pages
- **HasPreviousPage**: Whether previous page exists
- **HasNextPage**: Whether next page exists

### 3. PaginationResponse<T>
API response wrapper with:
- **Data**: The data items
- **Meta**: Pagination metadata
- **Links**: Navigation links (First, Last, Previous, Next)

### 4. PaginationHelper
Static helper class with utility methods:
- `CreatePaginatedResultAsync<T>()`: Creates paginated result from IQueryable
- `CreatePaginatedResult<T>()`: Creates paginated result from IEnumerable
- `ApplySearch<T>()`: Applies search filter to queryable
- `ApplySorting<T>()`: Applies sorting to queryable
- `ValidatePagination()`: Validates pagination parameters
- `GetPaginationMetadata()`: Gets pagination metadata

### 5. PaginationExtensions
Extension methods for easy pagination:
- `ToPaginatedResultAsync<T>()`: Extension for IQueryable
- `ToPaginatedResult<T>()`: Extension for IEnumerable
- `WhereSearch<T>()`: Search extension
- `OrderByField<T>()`: Sorting extension
- `OrderByDynamic<T>()`: Dynamic sorting extension

### 6. PaginationValidator
FluentValidation validator for pagination parameters.

## Usage Examples

### Basic Pagination
```csharp
// Query
public class GetAllCustomersQuery : PaginationDto, IQuery<PaginatedResult<CustomerDto>>
{
    // Additional filters...
}

// Handler
public async Task<PaginatedResult<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
{
    var query = _customerRepository.GetQueryable();
    
    // Apply search
    if (!string.IsNullOrWhiteSpace(request.SearchTerm))
    {
        query = query.WhereSearch(request.SearchTerm,
            x => x.FirstName,
            x => x.LastName,
            x => x.Email!.Value);
    }
    
    // Apply sorting
    query = query.OrderByField(request.SortBy, request.SortDirection, x => x.CreatedAt);
    
    // Get paginated result
    var result = await query.ToPaginatedResultAsync(request);
    
    // Map to DTOs
    var customerDtos = _mapper.Map<IEnumerable<CustomerDto>>(result.Data);
    
    return PaginatedResult<CustomerDto>.Success(customerDtos, result.TotalCount, result.PageNumber, result.PageSize);
}
```

### Advanced Filtering
```csharp
// Apply multiple filters
query = query.Where(x => x.Status == EntityStatus.Active)
             .Where(x => x.IsVerified == true)
             .Where(x => x.CreatedAt >= request.CreatedFrom)
             .Where(x => x.CreatedAt <= request.CreatedTo);
```

### Dynamic Sorting
```csharp
// Sort by any field dynamically
query = query.OrderByDynamic(request.SortBy, request.SortDirection);
```

### Search Across Multiple Fields
```csharp
// Search in multiple properties
query = query.WhereSearch(request.SearchTerm,
    x => x.FirstName,
    x => x.LastName,
    x => x.Email!.Value,
    x => x.PhoneNumber!.Value);
```

## API Response Format

### Success Response
```json
{
  "data": [
    {
      "id": "guid",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com"
    }
  ],
  "meta": {
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasPreviousPage": false,
    "hasNextPage": true,
    "startIndex": 1,
    "endIndex": 10
  },
  "links": {
    "first": "/api/customers?pageNumber=1&pageSize=10",
    "last": "/api/customers?pageNumber=10&pageSize=10",
    "previous": null,
    "next": "/api/customers?pageNumber=2&pageSize=10"
  }
}
```

### Error Response
```json
{
  "isSuccess": false,
  "errorMessage": "Validation failed",
  "errorCode": "VALIDATION_ERROR"
}
```

## Query Parameters

### Pagination Parameters
- `pageNumber`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `searchTerm`: Search term for filtering
- `sortBy`: Field to sort by
- `sortDirection`: Sort direction (asc/desc)

### Filter Parameters
- `status`: Customer status filter
- `customerType`: Customer type (regular/premium/business)
- `isVerified`: Verification status filter
- `isPremium`: Premium status filter
- `createdFrom`: Created date from
- `createdTo`: Created date to
- `minAge`: Minimum age filter
- `maxAge`: Maximum age filter
- `country`: Country filter
- `city`: City filter

### Additional Parameters
- `include`: Related entities to include (comma-separated)
- `select`: Specific fields to select (comma-separated)

## Example API Calls

### Basic Pagination
```
GET /api/customers?pageNumber=1&pageSize=10
```

### Search and Sort
```
GET /api/customers?pageNumber=1&pageSize=10&searchTerm=john&sortBy=firstName&sortDirection=asc
```

### Advanced Filtering
```
GET /api/customers?pageNumber=1&pageSize=10&status=active&isVerified=true&customerType=premium&createdFrom=2024-01-01&createdTo=2024-12-31&country=Iran&city=Tehran
```

### Include Related Data
```
GET /api/customers?pageNumber=1&pageSize=10&include=orders,addresses&select=id,firstName,lastName,email
```

## Best Practices

### 1. Performance
- Always use `IQueryable` for database queries
- Apply filters before pagination
- Use appropriate indexes on filtered fields
- Limit page size to reasonable values

### 2. Validation
- Always validate pagination parameters
- Use FluentValidation for complex validation rules
- Provide meaningful error messages

### 3. Security
- Validate sort fields to prevent SQL injection
- Sanitize search terms
- Limit maximum page size

### 4. User Experience
- Provide navigation links
- Include metadata for UI pagination controls
- Support flexible filtering options

## Extending the System

### Adding New Filters
1. Add filter properties to your Query DTO
2. Implement filter logic in the Handler
3. Add validation rules in the Validator

### Custom Search Logic
1. Override the `WhereSearch` method
2. Implement custom search expressions
3. Use `Expression<Func<T, object>>` for type-safe property access

### Custom Sorting
1. Use `OrderByField` for simple sorting
2. Use `OrderByDynamic` for dynamic sorting
3. Implement custom sorting logic for complex scenarios

## Troubleshooting

### Common Issues

1. **Performance Issues**
   - Check if filters are applied before pagination
   - Verify database indexes exist
   - Monitor query execution plans

2. **Validation Errors**
   - Check pagination parameter validation
   - Verify filter value formats
   - Review error messages

3. **Sorting Issues**
   - Ensure sort field exists in entity
   - Check sort direction values (asc/desc)
   - Verify case sensitivity

4. **Search Issues**
   - Check search property names
   - Verify search term encoding
   - Test with different character sets

## Migration Guide

### From Simple Pagination
1. Replace manual pagination with `ToPaginatedResultAsync`
2. Add search capabilities using `WhereSearch`
3. Implement sorting with `OrderByField`
4. Add validation using `PaginationValidator`

### From Custom Pagination
1. Replace custom pagination logic with standard components
2. Migrate filters to use the new system
3. Update API responses to use `PaginationResponse<T>`
4. Add comprehensive validation

## Conclusion

The pagination system provides a robust, flexible, and reusable solution for handling large datasets. It follows Clean Architecture principles and integrates seamlessly with the existing application structure. The system is designed to be extensible and maintainable, supporting both simple and complex pagination scenarios. 