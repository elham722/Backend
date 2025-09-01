using Backend.Application.Common.Validation;
using FluentValidation;

namespace Backend.Application.Features.UserManagement.Commands.Login;

/// <summary>
/// Validator for LoginCommand
/// </summary>
public class LoginCommandValidator : BaseValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        // ????? ?? ???????
        ValidateRequiredString(x => x.EmailOrUsername, 256);

        // ?????
        ValidateRequiredString(x => x.Password, 128);

        // ?? ?? ?????
        ValidateOptionalString(x => x.TwoFactorCode, 10)
            .Matches(@"^\d+$").WithMessage("Two-factor code must contain only digits");
    }

}