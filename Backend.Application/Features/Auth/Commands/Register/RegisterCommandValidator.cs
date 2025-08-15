using Backend.Application.Common.Validation;
using FluentValidation;

namespace Backend.Application.Features.Auth.Commands.Register
{
    /// <summary>
    /// Validator for RegisterCommand
    /// </summary>
    public class RegisterCommandValidator : BaseValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            // Username validation
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required")
                .Length(3, 50)
                .WithMessage("Username must be between 3 and 50 characters")
                .Matches(@"^[a-zA-Z0-9._-]+$")
                .WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Please provide a valid email address");

            // Password validation
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .Length(8, 128)
                .WithMessage("Password must be between 8 and 128 characters");

            // Custom password strength validation
            RuleFor(x => x.Password)
                .Must(BeStrongPassword)
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")
                .When(x => !string.IsNullOrEmpty(x.Password));

            // Phone number validation (optional)
            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .WithMessage("Phone number cannot exceed 20 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            // Custom phone number format validation
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Please provide a valid phone number in international format (e.g., +1234567890)")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            // Custom validation for username uniqueness (will be checked in handler)
            RuleFor(x => x.UserName)
                .MustAsync(async (userName, cancellation) =>
                {
                    // This will be validated in the handler against the database
                    return true; // Placeholder - actual validation in handler
                })
                .WithMessage("Username is already taken");

            // Custom validation for email uniqueness (will be checked in handler)
            RuleFor(x => x.Email)
                .MustAsync(async (email, cancellation) =>
                {
                    // This will be validated in the handler against the database
                    return true; // Placeholder - actual validation in handler
                })
                .WithMessage("Email is already registered");
        }

        private static bool BeStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            var hasUpperCase = password.Any(char.IsUpper);
            var hasLowerCase = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }
    }
} 