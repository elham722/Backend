using Backend.Application.Common.Validation;
using Backend.Application.Features.Customer.Queries.GetAllCustomers;
using FluentValidation;

namespace Backend.Application.Features.Customer.Queries.GetAllCustomers;

/// <summary>
/// Validator for GetAllCustomersQuery
/// </summary>
public class GetAllCustomersQueryValidator : BaseValidator<GetAllCustomersQuery>
{
    public GetAllCustomersQueryValidator()
    {
        // Pagination validation is inherited from PaginationDto

        // Status validation
        RuleFor(x => x.Status)
            .Must(BeValidCustomerStatus)
            .WithMessage("Invalid customer status")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        // Customer type validation
        RuleFor(x => x.CustomerType)
            .Must(BeValidCustomerType)
            .WithMessage("Customer type must be 'regular', 'premium', or 'business'")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerType));

        // Date range validation
        RuleFor(x => x.CreatedFrom)
            .LessThanOrEqualTo(x => x.CreatedTo)
            .WithMessage("CreatedFrom must be less than or equal to CreatedTo")
            .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue);

        // Age range validation
        RuleFor(x => x.MinAge)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum age must be greater than or equal to 0")
            .When(x => x.MinAge.HasValue);

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge)
            .WithMessage("Maximum age must be greater than or equal to minimum age")
            .When(x => x.MaxAge.HasValue && x.MinAge.HasValue);

        RuleFor(x => x.MaxAge)
            .LessThanOrEqualTo(150)
            .WithMessage("Maximum age cannot exceed 150")
            .When(x => x.MaxAge.HasValue);

        // Country validation
        RuleFor(x => x.Country)
            .MaximumLength(100)
            .WithMessage("Country cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        // City validation
        RuleFor(x => x.City)
            .MaximumLength(100)
            .WithMessage("City cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.City));
    }

    private static bool BeValidCustomerStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return true;

        return Enum.TryParse<Backend.Domain.Enums.CustomerStatus>(status, true, out _);
    }

    private static bool BeValidCustomerType(string? customerType)
    {
        if (string.IsNullOrWhiteSpace(customerType))
            return true;

        var validTypes = new[] { "regular", "premium", "business" };
        return validTypes.Contains(customerType.ToLower());
    }
} 