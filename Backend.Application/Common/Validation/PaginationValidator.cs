using Backend.Application.Common.DTOs;
using FluentValidation;

namespace Backend.Application.Common.Validation;

/// <summary>
/// Validator for pagination parameters
/// </summary>
public class PaginationValidator : AbstractValidator<PaginationDto>
{
    public PaginationValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .WithMessage("Search term cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));

        RuleFor(x => x.SortBy)
            .MaximumLength(50)
            .WithMessage("Sort field cannot exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy));

        RuleFor(x => x.SortDirection)
            .Must(BeValidSortDirection)
            .WithMessage("Sort direction must be 'asc' or 'desc'")
            .When(x => !string.IsNullOrWhiteSpace(x.SortDirection));
    }

    private static bool BeValidSortDirection(string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
            return true;

        return string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
    }
} 