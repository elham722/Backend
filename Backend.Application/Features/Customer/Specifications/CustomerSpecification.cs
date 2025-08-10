using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Specifications;
using System.Linq.Expressions;

namespace Backend.Application.Features.Customer.Specifications;

/// <summary>
/// Specification for customer filtering and pagination
/// </summary>
public class CustomerSpecification : BaseSpecification<Domain.Entities.Customer>
{
    public CustomerSpecification(
        string? searchTerm = null,
        string? status = null,
        string? customerType = null,
        bool? isVerified = null,
        bool? isPremium = null,
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        int? minAge = null,
        int? maxAge = null,
        string? country = null,
        string? city = null,
        string? sortBy = null,
        string? sortDirection = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        // Build a single combined criteria expression
        var combinedCriteria = BuildCombinedCriteria(
            searchTerm, status, customerType, isVerified, isPremium,
            createdFrom, createdTo, minAge, maxAge, country, city);

        if (combinedCriteria != null)
        {
            AddCriteria(combinedCriteria);
        }

        // Apply sorting
        ApplySorting(sortBy, sortDirection);

        // Apply pagination
        ApplyPaging(pageNumber, pageSize);
    }

    /// <summary>
    /// Builds a combined criteria expression for all filters
    /// </summary>
    private Expression<Func<Domain.Entities.Customer, bool>>? BuildCombinedCriteria(
        string? searchTerm,
        string? status,
        string? customerType,
        bool? isVerified,
        bool? isPremium,
        DateTime? createdFrom,
        DateTime? createdTo,
        int? minAge,
        int? maxAge,
        string? country,
        string? city)
    {
        var criteria = new List<Expression<Func<Domain.Entities.Customer, bool>>>();

        // Add search criteria
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            criteria.Add(x => 
                x.FirstName.ToLower().Contains(searchTermLower) ||
                x.LastName.ToLower().Contains(searchTermLower) ||
                (x.Email != null && x.Email.Value.ToLower().Contains(searchTermLower)) ||
                (x.PhoneNumber != null && x.PhoneNumber.Value.ToLower().Contains(searchTermLower)));
        }

        // Add status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<CustomerStatus>(status, true, out var customerStatus))
            {
                criteria.Add(x => x.CustomerStatus == customerStatus);
            }
        }

        // Add customer type filter
        if (!string.IsNullOrWhiteSpace(customerType))
        {
            switch (customerType.ToLower())
            {
                case "premium":
                    criteria.Add(x => x.IsPremium);
                    break;
                case "business":
                    criteria.Add(x => x.IsBusinessCustomer());
                    break;
                case "regular":
                    criteria.Add(x => !x.IsPremium && !x.IsBusinessCustomer());
                    break;
            }
        }

        // Add verification filter
        if (isVerified.HasValue)
        {
            criteria.Add(x => x.IsVerified == isVerified.Value);
        }

        // Add premium filter
        if (isPremium.HasValue)
        {
            criteria.Add(x => x.IsPremium == isPremium.Value);
        }

        // Add date range filter
        if (createdFrom.HasValue)
        {
            criteria.Add(x => x.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            criteria.Add(x => x.CreatedAt <= createdTo.Value);
        }

        // Add age range filter
        if (minAge.HasValue)
        {
            var minDate = DateTime.UtcNow.AddYears(-minAge.Value);
            criteria.Add(x => x.DateOfBirth <= minDate);
        }

        if (maxAge.HasValue)
        {
            var maxDate = DateTime.UtcNow.AddYears(-maxAge.Value);
            criteria.Add(x => x.DateOfBirth >= maxDate);
        }

        // Add location filters
        if (!string.IsNullOrWhiteSpace(country))
        {
            criteria.Add(x => x.PrimaryAddress != null && 
                           x.PrimaryAddress.Country.Contains(country));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            criteria.Add(x => x.PrimaryAddress != null && 
                           x.PrimaryAddress.City.Contains(city));
        }

        // For now, return the first criteria or null if none
        // This will be improved in the Infrastructure layer
        return criteria.FirstOrDefault();
    }



    /// <summary>
    /// Applies sorting based on field and direction
    /// </summary>
    private void ApplySorting(string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            // Default sorting
            AddOrderByDescending(x => x.CreatedAt); // Descending by default
            return;
        }

        var ascending = !string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        switch (sortBy.ToLower())
        {
            case "firstname":
                if (ascending)
                    AddOrderBy(x => x.FirstName);
                else
                    AddOrderByDescending(x => x.FirstName);
                break;
            case "lastname":
                if (ascending)
                    AddOrderBy(x => x.LastName);
                else
                    AddOrderByDescending(x => x.LastName);
                break;
            case "email":
                if (ascending)
                    AddOrderBy(x => x.Email!.Value);
                else
                    AddOrderByDescending(x => x.Email!.Value);
                break;
            case "createdat":
                if (ascending)
                    AddOrderBy(x => x.CreatedAt);
                else
                    AddOrderByDescending(x => x.CreatedAt);
                break;
            case "dateofbirth":
                if (ascending)
                    AddOrderBy(x => x.DateOfBirth);
                else
                    AddOrderByDescending(x => x.DateOfBirth);
                break;
            case "status":
                if (ascending)
                    AddOrderBy(x => x.CustomerStatus);
                else
                    AddOrderByDescending(x => x.CustomerStatus);
                break;
            default:
                // Default sorting if field not recognized
                AddOrderByDescending(x => x.CreatedAt);
                break;
        }
    }
} 