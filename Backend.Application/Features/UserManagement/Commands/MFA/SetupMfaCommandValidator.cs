using Backend.Application.Common.Validation;
using Backend.Domain.Enums;
using FluentValidation;

namespace Backend.Application.Features.UserManagement.Commands.MFA;

/// <summary>
/// Validator for SetupMfaCommand
/// </summary>
public class SetupMfaCommandValidator : BaseValidator<SetupMfaCommand>
{
    public SetupMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid MFA type");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().When(x => x.Type == MfaType.SMS)
            .WithMessage("Phone number is required for SMS MFA")
            .Matches(@"^(\+98|0)?9\d{9}$").When(x => x.Type == MfaType.SMS)
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("IP address is required for security audit");

        RuleFor(x => x.DeviceInfo)
            .NotEmpty().WithMessage("Device information is required for security audit");
    }
} 