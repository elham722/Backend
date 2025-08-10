using Backend.Application.Common.Commands;
using Backend.Application.Common.Results;

namespace Backend.Application.Features.Customer.Commands.DeleteCustomer;

/// <summary>
/// Command for deleting a customer
/// </summary>
public class DeleteCustomerCommand : ICommand<Result>
{
    /// <summary>
    /// Customer ID
    /// </summary>
    public Guid Id { get; set; }
} 