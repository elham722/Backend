# BaseValidator Documentation

`BaseValidator<T>` یک کلاس پایه برای ایجاد validators در اپلیکیشن است که از FluentValidation استفاده می‌کند.

## متدهای موجود

### 1. ValidateRequiredString
برای اعتبارسنجی فیلدهای string اجباری استفاده می‌شود.

```csharp
ValidateRequiredString(x => x.FirstName, 50, 2)
    .WithMessage("First name must be between 2 and 50 characters");
```

**پارامترها:**
- `expression`: Expression برای property
- `maxLength`: حداکثر طول (پیش‌فرض: 255)
- `minLength`: حداقل طول (پیش‌فرض: 1)

### 2. ValidateEmail
برای اعتبارسنجی آدرس ایمیل استفاده می‌شود.

```csharp
ValidateEmail(x => x.Email);
```

### 3. ValidatePhoneNumber
برای اعتبارسنجی شماره تلفن استفاده می‌شود.

```csharp
ValidatePhoneNumber(x => x.PhoneNumber);
```

### 4. ValidateRequired
برای اعتبارسنجی فیلدهای اجباری از هر نوع استفاده می‌شود.

```csharp
ValidateRequired(x => x.Id)
    .WithMessage("ID is required");
```

### 5. ValidateOptionalString
برای اعتبارسنجی فیلدهای string اختیاری استفاده می‌شود.

```csharp
ValidateOptionalString(x => x.Address, 500)
    .WithMessage("Address cannot exceed 500 characters");
```

## مثال کامل

```csharp
public class CreateCustomerCommandValidator : BaseValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        // Required string validation
        ValidateRequiredString(x => x.FirstName, 50, 2)
            .WithMessage("First name must be between 2 and 50 characters");

        // Email validation
        ValidateEmail(x => x.Email);

        // Phone number validation
        ValidatePhoneNumber(x => x.PhoneNumber);

        // Custom validation
        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .WithMessage("Date of birth cannot be in the future")
            .When(x => x.DateOfBirth.HasValue);

        // Optional string validation
        ValidateOptionalString(x => x.Address, 500)
            .WithMessage("Address cannot exceed 500 characters");
    }
}
```

## مزایا

1. **کد کمتر**: نیازی به نوشتن validation rules تکراری نیست
2. **یکپارچگی**: تمام validators از قوانین یکسان پیروی می‌کنند
3. **قابلیت نگهداری**: تغییرات در یک مکان اعمال می‌شود
4. **قابلیت گسترش**: می‌توان متدهای جدید اضافه کرد
5. **Type Safety**: از generic constraints استفاده می‌کند

## نکات مهم

- متدهای `ValidateRequiredString`، `ValidateEmail` و `ValidatePhoneNumber` فقط برای `string` properties کار می‌کنند
- متد `ValidateRequired` برای هر نوع property قابل استفاده است
- متد `ValidateOptionalString` برای `string?` properties استفاده می‌شود
- می‌توانید از `RuleFor` معمولی FluentValidation برای validation های پیچیده استفاده کنید 