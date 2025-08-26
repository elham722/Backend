using FluentValidation;

namespace Backend.Application.Features.UserManagement.Commands.UpdateUser;

/// <summary>
/// Validator for UpdateUserCommand
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format")
            .MaximumLength(256).When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.UserName)
            .MinimumLength(3).When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Username cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_-]+$").When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Username can only contain letters, numbers, underscores, and hyphens");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number cannot exceed 20 characters")
            .Matches(@"^[\+]?[1-9][\d]{0,15}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy is required");
    }
} 