# راهنمای Mapping برای لایه Identity

## مقدمه

این راهنما نحوه استفاده از mapper های Identity را در پروژه Backend توضیح می‌دهد. این راه حل از اصول Clean Architecture پیروی می‌کند و وابستگی بین لایه‌ها را به حداقل می‌رساند.

## معماری

### لایه Application
- **Interfaces**: تعریف interface های Identity در `Backend.Application/Common/Interfaces/Identity/`
- **DTOs**: تعریف DTO های مربوط به Identity در `Backend.Application/Features/UserManagement/DTOs/Identity/`
- **Mappers**: تعریف AutoMapper profiles در `Backend.Application/Mappers/Profiles/IdentityMappingProfile.cs`

### لایه Infrastructure
- **Adapters**: پیاده‌سازی adapter های Identity در `Backend.Infrastructure/Adapters/Identity/`
- **Factory**: factory برای ایجاد adapter ها در `IdentityAdapterFactory.cs`

## نحوه استفاده

### 1. استفاده در Query Handler

```csharp
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IdentityAdapterFactory _adapterFactory;

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // دریافت کاربر از Identity
        var user = await _userManager.FindByIdAsync(request.UserId);
        
        // تبدیل به interface با استفاده از adapter
        var userInterface = _adapterFactory.CreateApplicationUser(user);
        
        // تبدیل به DTO با استفاده از AutoMapper
        var userDto = _mapper.Map<UserDto>(userInterface);
        
        return ApiResponse<UserDto>.Success(userDto);
    }
}
```

### 2. استفاده در Command Handler

```csharp
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IdentityAdapterFactory _adapterFactory;

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        
        // به‌روزرسانی کاربر
        user.Email = request.Email;
        await _userManager.UpdateAsync(user);
        
        // تبدیل به DTO
        var userInterface = _adapterFactory.CreateApplicationUser(user);
        var userDto = _mapper.Map<UserDto>(userInterface);
        
        return ApiResponse<UserDto>.Success(userDto);
    }
}
```

### 3. استفاده برای Collection ها

```csharp
public async Task<ApiResponse<List<UserDto>>> GetAllUsers()
{
    var users = _userManager.Users.ToList();
    
    // تبدیل collection به interface ها
    var userInterfaces = IdentityAdapterFactory.CreateApplicationUsers(users);
    
    // تبدیل به DTO ها
    var userDtos = _mapper.Map<List<UserDto>>(userInterfaces);
    
    return ApiResponse<List<UserDto>>.Success(userDtos);
}
```

## مزایای این راه حل

### 1. جداسازی لایه‌ها
- لایه Application به لایه Identity وابسته نیست
- فقط از interface ها استفاده می‌کند
- قابلیت تست‌پذیری بالا

### 2. انعطاف‌پذیری
- امکان تغییر پیاده‌سازی Identity بدون تأثیر بر Application
- امکان استفاده از mock objects در تست‌ها
- قابلیت توسعه آسان

### 3. سازگاری با Clean Architecture
- پیروی از Dependency Inversion Principle
- جداسازی concerns
- قابلیت نگهداری بالا

## Interface های موجود

### IApplicationUser
- تمام properties و methods مربوط به ApplicationUser
- شامل AccountInfo، SecurityInfo، و AuditInfo

### IUserClaim
- properties مربوط به UserClaim

### IUserToken
- properties مربوط به UserToken

### IUserRole
- properties مربوط به UserRole

### IUserLogin
- properties مربوط به UserLogin

## DTO های موجود

### UserDto
- DTO کامل برای اطلاعات کاربر
- شامل تمام properties مورد نیاز

### UserSummaryDto
- DTO خلاصه برای لیست کاربران
- شامل اطلاعات ضروری

### UserClaimDto, UserTokenDto, UserRoleDto, UserLoginDto
- DTO های مربوط به هر entity

## نکات مهم

### 1. Dependency Injection
- `IdentityAdapterFactory` به صورت Singleton ثبت شده
- Adapter ها stateless هستند

### 2. Performance
- Adapter ها فقط interface mapping انجام می‌دهند
- overhead کمی دارند
- برای collection های بزرگ مناسب هستند

### 3. Testing
- امکان mock کردن interface ها
- تست‌های unit و integration آسان‌تر

## مثال کامل

### 1. استفاده در Query Handler

```csharp
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly IdentityAdapterFactory _adapterFactory;

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // دریافت کاربر از Identity
        var user = await _userManager.FindByIdAsync(request.UserId);
        
        // دریافت نقش‌های کاربر
        var roles = await _userManager.GetRolesAsync(user);
        user.Roles = roles.ToList();
        
        // تبدیل به interface با استفاده از adapter
        var userInterface = _adapterFactory.CreateApplicationUser(user);
        
        // تبدیل به DTO با استفاده از AutoMapper
        var userDto = _mapper.Map<UserDto>(userInterface);
        
        return ApiResponse<UserDto>.Success(userDto);
    }
}
```

### 2. استفاده در UserService (ریفکتور شده)

```csharp
public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IdentityAdapterFactory _adapterFactory;
    
    public async Task<Result<UserDto>> GetUserByIdAsync(string userId, bool includeRoles = true)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result<UserDto>.Failure("User not found", "UserNotFound");
        }

        if (includeRoles)
        {
            var roles = await _userManager.GetRolesAsync(user);
            user.Roles = roles.ToList();
        }

        // استفاده از adapter و AutoMapper
        var userInterface = _adapterFactory.CreateApplicationUser(user);
        var userDto = _mapper.Map<UserDto>(userInterface);
        
        return Result<UserDto>.Success(userDto);
    }
}
```

### 3. استفاده در Controller

```csharp
[HttpGet("{userId}")]
public async Task<IActionResult> GetUser(string userId)
{
    var query = new GetUserByIdQuery { UserId = userId };
    var result = await _mediator.Send(query);
    
    if (result.IsSuccess)
    {
        return Ok(result.Data);
    }
    
    return NotFound(result.Message);
}
```

## تغییرات انجام شده در UserService

### قبل از ریفکتور:
```csharp
private readonly IUserMapper _userMapper;

// استفاده
var userDto = _userMapper.MapToUserDto(user, roles.ToList());
```

### بعد از ریفکتور:
```csharp
private readonly IMapper _mapper;
private readonly IdentityAdapterFactory _adapterFactory;

// استفاده
var userInterface = _adapterFactory.CreateApplicationUser(user);
var userDto = _mapper.Map<UserDto>(userInterface);
```

## مزایای ریفکتور

1. **حذف وابستگی**: دیگر نیازی به `IUserMapper` نیست
2. **استفاده از AutoMapper**: mapping قدرتمندتر و قابل تنظیم‌تر
3. **سازگاری با Clean Architecture**: جداسازی کامل لایه‌ها
4. **قابلیت تست‌پذیری**: امکان mock کردن interface ها
5. **انعطاف‌پذیری**: امکان اضافه کردن mapping های جدید

این راه حل به شما امکان استفاده از mapper های Identity را بدون نقض اصول Clean Architecture می‌دهد.