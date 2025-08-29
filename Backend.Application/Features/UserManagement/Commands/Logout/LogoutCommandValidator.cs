using Backend.Application.Common.Validation;
using FluentValidation;

namespace Backend.Application.Features.UserManagement.Commands.Logout
{
    /// <summary>
    /// Validator for LogoutCommand
    /// </summary>
    public class LogoutCommandValidator : BaseValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token is required")
                .When(x => !x.LogoutFromAllDevices)
                .WithErrorCode("LOGOUT_001");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required when logging out from all devices")
                .When(x => x.LogoutFromAllDevices)
                .WithErrorCode("LOGOUT_002");

            RuleFor(x => x.IpAddress)
                .MaximumLength(45)
                .WithMessage("IP address is too long")
                .When(x => !string.IsNullOrEmpty(x.IpAddress))
                .WithErrorCode("LOGOUT_003");

            RuleFor(x => x.UserAgent)
                .MaximumLength(500)
                .WithMessage("User agent is too long")
                .When(x => !string.IsNullOrEmpty(x.UserAgent))
                .WithErrorCode("LOGOUT_004");

            RuleFor(x => x.LogoutReason)
                .MaximumLength(200)
                .WithMessage("Logout reason is too long")
                .When(x => !string.IsNullOrEmpty(x.LogoutReason))
                .WithErrorCode("LOGOUT_005");
        }
    }
} 