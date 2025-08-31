using Backend.Application.Common.Validation;
using Backend.Domain.Enums;
using FluentValidation;

namespace Backend.Application.Features.UserManagement.Commands.MFA;

/// <summary>
/// Validator for VerifyMfaCommand
/// </summary>
public class VerifyMfaCommandValidator : BaseValidator<VerifyMfaCommand>
{
    public VerifyMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid MFA type");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .MinimumLength(4).WithMessage("Code must be at least 4 characters")
            .MaximumLength(10).WithMessage("Code cannot exceed 10 characters");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP address is required for security audit");
    }
} 