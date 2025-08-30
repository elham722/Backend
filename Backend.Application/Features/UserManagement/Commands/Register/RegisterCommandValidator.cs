using FluentValidation;

namespace Backend.Application.Features.UserManagement.Commands.Register;

/// <summary>
/// Validator for RegisterCommand
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores, and hyphens");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")
            .Must(password => 
            {
                if (string.IsNullOrEmpty(password)) return false;
                var uniqueChars = password.Distinct().Count();
                return uniqueChars >= 4;
            })
            .WithMessage("Password must contain at least 4 different character types");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^[\+]?[1-9][\d]{0,15}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("Terms and conditions must be accepted");

        RuleFor(x => x.CaptchaChallengeId)
            .NotEmpty().WithMessage("CAPTCHA challenge is required")
            .MaximumLength(100).WithMessage("CAPTCHA challenge ID is too long");

        RuleFor(x => x.CaptchaAnswer)
            .NotEmpty().WithMessage("CAPTCHA answer is required")
            .MaximumLength(10).WithMessage("CAPTCHA answer is too long");
    }
} 