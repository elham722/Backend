using Backend.Application.Common.DTOs;
using Backend.Application.Common.Queries;
using Backend.Application.Common.Results;
using Backend.Application.Features.Customer.DTOs;

namespace Backend.Application.Features.Customer.Queries.GetAllCustomers;

/// <summary>
/// Query for getting all customers with pagination
/// </summary>
public class GetAllCustomersQuery : PaginationDto, IQuery<PaginatedResult<CustomerDto>>
{
    /// <summary>
    /// Filter by status
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Filter by verification status
    /// </summary>
    public bool? IsVerified { get; set; }
    
    /// <summary>
    /// Filter by premium status
    /// </summary>
    public bool? IsPremium { get; set; }
    
    /// <summary>
    /// Filter by minimum age
    /// </summary>
    public int? MinAge { get; set; }
    
    /// <summary>
    /// Filter by maximum age
    /// </summary>
    public int? MaxAge { get; set; }
} 