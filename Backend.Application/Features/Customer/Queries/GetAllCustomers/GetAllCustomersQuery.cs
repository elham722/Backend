using Backend.Application.Common.DTOs;
using Backend.Application.Common.Queries;
using Backend.Application.Common.Results;
using Backend.Application.Features.Customer.DTOs;

namespace Backend.Application.Features.Customer.Queries.GetAllCustomers;

/// <summary>
/// Query for getting all customers with pagination and filtering
/// </summary>
public class GetAllCustomersQuery : PaginationDto, IQuery<PaginatedResult<CustomerDto>>
{
    /// <summary>
    /// Filter by customer status
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by customer type (regular, premium, business)
    /// </summary>
    public string? CustomerType { get; set; }

    /// <summary>
    /// Filter by verification status
    /// </summary>
    public bool? IsVerified { get; set; }

    /// <summary>
    /// Filter by premium status
    /// </summary>
    public bool? IsPremium { get; set; }

    /// <summary>
    /// Filter by date range - from
    /// </summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>
    /// Filter by date range - to
    /// </summary>
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Filter by age range - minimum
    /// </summary>
    public int? MinAge { get; set; }

    /// <summary>
    /// Filter by age range - maximum
    /// </summary>
    public int? MaxAge { get; set; }

    /// <summary>
    /// Filter by country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Filter by city
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Include related entities (comma-separated)
    /// </summary>
    public string? Include { get; set; }
} 