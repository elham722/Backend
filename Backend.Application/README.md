# Backend.Application Layer

این لایه مسئول منطق اپلیکیشن و orchestration بین لایه‌های مختلف است. این لایه از اصول Clean Architecture و Domain-Driven Design پیروی می‌کند.

## ساختار پوشه‌ها

```
Backend.Application/
├── Common/                    # کلاس‌ها و interface های مشترک
│   ├── Behaviors/            # Pipeline behaviors برای MediatR
│   ├── Commands/             # Base command interfaces
│   ├── DTOs/                 # Data Transfer Objects پایه
│   ├── Extensions/           # Extension methods
│   ├── Interfaces/           # Application service interfaces
│   ├── Queries/              # Base query interfaces
│   ├── Results/              # Result wrapper classes
│   └── Validation/           # Base validation classes
├── DependencyInjection/      # Service registration
├── Features/                 # Feature-based organization
│   └── Customer/             # Customer feature
│       ├── Commands/         # Customer commands
│       ├── DTOs/             # Customer DTOs
│       ├── Queries/          # Customer queries
│       ├── Specifications/   # Customer specifications
│       └── Validators/       # Customer validators
├── Mappers/                  # AutoMapper profiles
│   └── Profiles/
└── Services/                 # Application services
```

## اصول طراحی

### 1. CQRS Pattern
- **Commands**: برای عملیات‌های تغییردهنده (Create, Update, Delete)
- **Queries**: برای عملیات‌های خواندن (Read)

### 2. MediatR Pipeline
- **ValidationBehavior**: اعتبارسنجی درخواست‌ها
- **LoggingBehavior**: ثبت لاگ‌ها
- **CachingBehavior**: کش کردن نتایج queries

### 3. Result Pattern
- **Result<T>**: برای عملیات‌هایی که داده برمی‌گردانند
- **Result**: برای عملیات‌هایی که فقط موفقیت/شکست برمی‌گردانند
- **PaginatedResult<T>**: برای نتایج صفحه‌بندی شده

### 4. Validation
- استفاده از FluentValidation
- BaseValidator برای validation rules مشترک
- Validation در pipeline MediatR

### 5. AutoMapper
- Mapping بین Entity ها و DTO ها
- Profile های جداگانه برای هر entity

## نحوه استفاده

### 1. ایجاد Command جدید

```csharp
public class CreateCustomerCommand : ICommand<Result<CustomerDto>>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### 2. ایجاد Query جدید

```csharp
public class GetCustomerByIdQuery : IQuery<Result<CustomerDto>>
{
    public Guid Id { get; set; }
}

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### 3. ایجاد Validator

```csharp
public class CreateCustomerCommandValidator : BaseValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        ValidateRequiredString(x => x.FirstName, 50, 2);
        ValidateEmail(x => x.Email);
        ValidatePhoneNumber(x => x.PhoneNumber);
    }
}
```

### 4. ایجاد Specification

```csharp
public class CustomerSpecification : BaseSpecification<Customer>
{
    public CustomerSpecification(string? searchTerm = null, string? status = null)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            AddCriteria(c => c.FirstName.Contains(searchTerm));
        }
        
        if (!string.IsNullOrWhiteSpace(status))
        {
            AddCriteria(c => c.Status.ToString() == status);
        }
    }
}
```

## ثبت سرویس‌ها

```csharp
// در Program.cs یا Startup.cs
services.AddApplicationServices();
```

## مزایای این معماری

1. **Separation of Concerns**: جداسازی منطق اپلیکیشن از سایر لایه‌ها
2. **Testability**: قابلیت تست آسان با استفاده از dependency injection
3. **Maintainability**: کد تمیز و قابل نگهداری
4. **Scalability**: قابلیت گسترش آسان
5. **Consistency**: یکپارچگی در سراسر اپلیکیشن
6. **Validation**: اعتبارسنجی متمرکز و قابل تنظیم
7. **Caching**: کش کردن هوشمند نتایج
8. **Logging**: ثبت لاگ‌های متمرکز 