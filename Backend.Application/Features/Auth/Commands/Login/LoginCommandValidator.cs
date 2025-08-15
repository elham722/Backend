using Backend.Application.Common.Validation;
using FluentValidation;

namespace Backend.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Validator for LoginCommand
    /// </summary>
    public class LoginCommandValidator : BaseValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            // Username validation
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required")
                .Length(3, 50)
                .WithMessage("Username must be between 3 and 50 characters")
                .Matches(@"^[a-zA-Z0-9._-]+$")
                .WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

            // Password validation
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .Length(6, 128)
                .WithMessage("Password must be between 6 and 128 characters");

            // Custom password strength validation
            RuleFor(x => x.Password)
                .Must(BeStrongPassword)
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")
                .When(x => !string.IsNullOrEmpty(x.Password));

            // RememberMe validation (optional boolean, no validation needed)
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