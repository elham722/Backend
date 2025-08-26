using FluentValidation;

namespace Backend.Application.Features.UserManagement.Commands.Login;

/// <summary>
/// Validator for LoginCommand
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("Email or username is required")
            .MaximumLength(256).WithMessage("Email or username cannot exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters");

        RuleFor(x => x.TwoFactorCode)
            .MaximumLength(10).When(x => !string.IsNullOrEmpty(x.TwoFactorCode))
            .WithMessage("Two-factor code cannot exceed 10 characters")
            .Matches(@"^\d+$").When(x => !string.IsNullOrEmpty(x.TwoFactorCode))
            .WithMessage("Two-factor code must contain only digits");
    }
} 