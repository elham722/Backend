using Backend.Application.Common.Queries;
using Backend.Application.Common.Results;
using Backend.Application.Features.Customer.DTOs;

namespace Backend.Application.Features.Customer.Queries.GetCustomerById;

/// <summary>
/// Query for getting a customer by ID
/// </summary>
public class GetCustomerByIdQuery : IQuery<Result<CustomerDto>>
{
    /// <summary>
    /// Customer ID
    /// </summary>
    public Guid Id { get; set; }
} 