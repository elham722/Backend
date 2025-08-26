# راهنمای معماری تمیز برای مدیریت کاربران

## مشکل حل شده
پروژه شما دارای دو لایه جداگانه برای مدیریت کاربران بود:
- **Backend.Identity**: برای احراز هویت و مجوزها
- **Backend.Domain**: برای منطق کسب‌وکار

این باعث تداخل مسئولیت‌ها و تکرار کد می‌شد.

## راه‌حل پیاده‌سازی شده

### 1. جداسازی مسئولیت‌ها

#### Backend.Domain (لایه هسته)
**مسئولیت‌ها:**
- موجودیت‌های اصلی کسب‌وکار (User, Customer, etc.)
- Value Objects مشترک (UserId, UserStatus, Email, etc.)
- Business Rules اصلی (UserBusinessRules)
- Domain Events

**فایل‌های جدید:**
```
Backend.Domain/
├── ValueObjects/User/
│   ├── UserId.cs
│   └── UserStatus.cs
├── BusinessRules/User/
│   └── UserBusinessRules.cs
├── Entities/
│   └── User.cs
└── Events/Identity/
    ├── UserCreatedEvent.cs
    ├── UserActivatedEvent.cs
    └── ...
```

#### Backend.Identity (لایه احراز هویت)
**مسئولیت‌ها:**
- مدیریت احراز هویت (Authentication)
- مدیریت مجوزها (Authorization)
- Value Objects مخصوص Identity
- Bridge Service برای ارتباط با Domain

**فایل‌های جدید:**
```
Backend.Identity/
├── Services/
│   └── UserDomainService.cs
└── ValueObjects/
    ├── AccountInfo.cs
    ├── SecurityInfo.cs
    └── AuditInfo.cs
```

### 2. Value Objects مشترک

#### UserId
```csharp
// در Backend.Domain/ValueObjects/User/UserId.cs
public class UserId : ValueObject
{
    public string Value { get; private set; }
    
    public static UserId Create(string value)
    public static UserId CreateFromGuid(Guid guid)
    public static UserId CreateFromEmail(string email)
}
```

#### UserStatus
```csharp
// در Backend.Domain/ValueObjects/User/UserStatus.cs
public class UserStatus : ValueObject
{
    public static readonly UserStatus Active = new UserStatus("Active");
    public static readonly UserStatus Inactive = new UserStatus("Inactive");
    public static readonly UserStatus Locked = new UserStatus("Locked");
    // ...
    
    public bool CanLogin => this == Active || this == Pending;
    public bool IsActive => this == Active;
}
```

### 3. Business Rules مشترک

```csharp
// در Backend.Domain/BusinessRules/User/UserBusinessRules.cs
public static class UserBusinessRules
{
    public static class Validation
    {
        public static void ValidateEmail(string email)
        public static void ValidateUsername(string username)
        public static void ValidatePassword(string password)
    }
    
    public static class Authorization
    {
        public static void EnsureUserCanLogin(UserStatus status)
        public static void EnsureUserIsActive(UserStatus status)
    }
    
    public static class Security
    {
        public static void ValidateLoginAttempts(int attempts)
        public static void ValidatePasswordAge(DateTime lastPasswordChange)
    }
}
```

### 4. موجودیت User در Domain

```csharp
// در Backend.Domain/Entities/User.cs
public class User : BaseAggregateRoot<Guid>
{
    public UserId UserId { get; private set; }
    public string Email { get; private set; }
    public UserStatus Status { get; private set; }
    
    // Business Methods
    public void Activate(string? activatedBy = null)
    public void Lock(string reason, DateTime? lockedUntil = null)
    public void RecordSuccessfulLogin()
    public void ValidateForLogin()
}
```

### 5. Bridge Service

```csharp
// در Backend.Identity/Services/UserDomainService.cs
public class UserDomainService : IUserDomainService
{
    public async Task<User> CreateUserAsync(string email, string username, ...)
    {
        // 1. Validate using Domain business rules
        UserBusinessRules.Validation.ValidateEmail(email);
        
        // 2. Create Domain User
        var domainUser = User.Create(email, username, ...);
        
        // 3. Create Identity User
        var identityUser = ApplicationUser.Create(email, username, ...);
        
        // 4. Save both
        await _userRepository.AddAsync(domainUser);
        await _userManager.CreateAsync(identityUser);
    }
}
```

## نحوه استفاده

### 1. ایجاد کاربر جدید
```csharp
// در Application Layer
public class CreateUserCommandHandler
{
    private readonly IUserDomainService _userDomainService;
    
    public async Task<Guid> Handle(CreateUserCommand command)
    {
        var user = await _userDomainService.CreateUserAsync(
            command.Email, 
            command.Username, 
            command.FirstName, 
            command.LastName
        );
        
        return user.Id;
    }
}
```

### 2. احراز هویت کاربر
```csharp
// در Identity Layer
public class AuthService : IAuthService
{
    private readonly IUserDomainService _userDomainService;
    
    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        // 1. Validate user can login using Domain rules
        var canLogin = await _userDomainService.ValidateUserForLoginAsync(email);
        if (!canLogin)
            return AuthResult.Failed("User cannot login");
            
        // 2. Authenticate with Identity
        var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
        
        if (result.Succeeded)
        {
            // 3. Record successful login in Domain
            await _userDomainService.RecordSuccessfulLoginAsync(email);
            return AuthResult.Success();
        }
        else
        {
            // 4. Record failed login in Domain
            await _userDomainService.RecordFailedLoginAsync(email);
            return AuthResult.Failed("Invalid credentials");
        }
    }
}
```

### 3. مدیریت وضعیت کاربر
```csharp
// در Application Layer
public class LockUserCommandHandler
{
    private readonly IUserDomainService _userDomainService;
    
    public async Task Handle(LockUserCommand command)
    {
        await _userDomainService.LockUserAsync(
            command.Email, 
            command.Reason, 
            command.LockedUntil, 
            command.LockedBy
        );
    }
}
```

## مزایای این معماری

### ✅ جداسازی مسئولیت‌ها
- Domain: منطق کسب‌وکار
- Identity: احراز هویت و مجوزها
- Application: هماهنگی بین لایه‌ها

### ✅ قابلیت توسعه
- Business Rules مشترک
- Value Objects قابل استفاده مجدد
- Domain Events برای loose coupling

### ✅ قابلیت نگهداری
- کد تمیز و قابل فهم
- تست‌پذیری بالا
- تغییرات محدود به لایه مربوطه

### ✅ انعطاف‌پذیری
- امکان تغییر Identity Provider
- امکان اضافه کردن Business Rules جدید
- امکان گسترش Value Objects

## مراحل بعدی

### 1. پیاده‌سازی Repository
```csharp
// در Backend.Persistence
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<User> GetByEmailAsync(string email)
    {
        // Implementation
    }
}
```

### 2. اضافه کردن Domain Events Handlers
```csharp
// در Backend.Application
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Send welcome email, create user profile, etc.
    }
}
```

### 3. اضافه کردن Validation
```csharp
// در Backend.Application
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Username).MinimumLength(3);
    }
}
```

## نکات مهم

1. **همگام‌سازی**: همیشه Domain و Identity باید همگام باشند
2. **Transaction**: عملیات Domain و Identity باید در یک transaction انجام شود
3. **Error Handling**: خطاهای Domain باید به درستی مدیریت شوند
4. **Performance**: از Caching برای بهبود عملکرد استفاده کنید
5. **Security**: Business Rules امنیتی را در Domain پیاده‌سازی کنید

این معماری به شما امکان می‌دهد تا پروژه‌تان را به صورت اصولی و قابل توسعه مدیریت کنید. 