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
            .NotEmpty().WithMessage("ایمیل الزامی است")
            .EmailAddress().WithMessage("فرمت ایمیل صحیح نیست")
            .MaximumLength(256).WithMessage("ایمیل نمی‌تواند بیش از 256 کاراکتر باشد");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("نام کاربری الزامی است")
            .MinimumLength(3).WithMessage("نام کاربری باید حداقل 3 کاراکتر باشد")
            .MaximumLength(50).WithMessage("نام کاربری نمی‌تواند بیش از 50 کاراکتر باشد")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("نام کاربری فقط می‌تواند شامل حروف، اعداد، خط تیره و زیرخط باشد");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است")
            .MinimumLength(8).WithMessage("رمز عبور باید حداقل 8 کاراکتر باشد")
            .MaximumLength(128).WithMessage("رمز عبور نمی‌تواند بیش از 128 کاراکتر باشد")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$")
            .WithMessage("رمز عبور باید شامل حداقل یک حرف بزرگ، یک حرف کوچک، یک عدد و یک کاراکتر خاص باشد")
            .Must(password => 
            {
                if (string.IsNullOrEmpty(password)) return false;
                var uniqueChars = password.Distinct().Count();
                return uniqueChars >= 4;
            })
            .WithMessage("رمز عبور باید شامل حداقل 4 نوع کاراکتر مختلف باشد");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("تکرار رمز عبور الزامی است")
            .Equal(x => x.Password).WithMessage("رمز عبور و تکرار آن مطابقت ندارند");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("شماره تلفن نمی‌تواند بیش از 20 کاراکتر باشد")
            .Matches(@"^09\d{9}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("شماره تلفن باید با 09 شروع شود و 11 رقم باشد");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("قوانین و شرایط باید پذیرفته شوند");

        RuleFor(x => x.CaptchaToken)
            .NotEmpty().WithMessage("تأیید CAPTCHA الزامی است")
            .MaximumLength(10000).WithMessage("توکن CAPTCHA خیلی طولانی است");
    }
} 