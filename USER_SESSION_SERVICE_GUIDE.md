# راهنمای UserSessionService

## خلاصه

`UserSessionService` یک سرویس مجزا برای مدیریت session کاربران است که مسئولیت‌های مربوط به session را از Controller ها جدا می‌کند.

## ساختار

### Interface
```csharp
public interface IUserSessionService
{
    void SetUserSession(AuthResultDto result);
    void ClearUserSession();
    string? GetUserId();
    string? GetUserName();
    string? GetUserEmail();
    string? GetJwtToken();
    string? GetRefreshToken();
    bool IsAuthenticated();
    LogoutDto GetLogoutDto();
}
```

### Implementation
```csharp
public class UserSessionService : IUserSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserSessionService> _logger;

    // Implementation methods...
}
```

## نحوه استفاده

### در Controller
```csharp
public class AuthController : BaseController
{
    private readonly IAuthApiClient _authApiClient;

    public AuthController(
        IAuthApiClient authApiClient, 
        IUserSessionService userSessionService,
        ILogger<AuthController> logger)
        : base(userSessionService, logger)
    {
        _authApiClient = authApiClient;
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var response = await _authApiClient.RegisterAsync(model);
        
        if (response.IsSuccess)
        {
            // Set user session using the service
            UserSessionService.SetUserSession(response);
            return RedirectToAction("Index", "Home");
        }
        
        return View(model);
    }
}
```

### در BaseController
```csharp
public abstract class BaseController : Controller
{
    protected readonly IUserSessionService UserSessionService;
    protected readonly ILogger Logger;

    // Properties for easy access
    protected string? CurrentUserId => UserSessionService.GetUserId();
    protected string? CurrentUserName => UserSessionService.GetUserName();
    protected bool IsAuthenticated => UserSessionService.IsAuthenticated();

    protected void SetUserViewBag()
    {
        ViewBag.IsLoggedIn = IsAuthenticated;
        ViewBag.UserName = CurrentUserName;
        ViewBag.UserEmail = CurrentUserEmail;
    }
}
```

### در Program.cs
```csharp
// Register session management service
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddHttpContextAccessor();
```

## مزایا

### ✅ Separation of Concerns
- جداسازی منطق session management از Controller
- مسئولیت‌های مشخص و جداگانه
- کد تمیزتر و قابل نگهداری‌تر

### ✅ Reusability
- امکان استفاده مجدد در همه Controller ها
- کاهش duplication در کد
- مدیریت متمرکز session

### ✅ Testability
- امکان mock کردن سرویس برای تست
- تست‌پذیری بهتر Controller ها
- Isolation مناسب

### ✅ Error Handling
- مدیریت خطا در یک مکان
- Logging متمرکز
- Exception handling یکپارچه

### ✅ Type Safety
- استفاده از DTO های مشخص
- Compile-time error detection
- IntelliSense support بهتر

## Session Keys

### JWT Token
```csharp
session.SetString("JWTToken", result.AccessToken);
```

### Refresh Token
```csharp
session.SetString("RefreshToken", result.RefreshToken);
```

### User Information
```csharp
session.SetString("UserName", result.User.UserName);
session.SetString("UserEmail", result.User.Email);
session.SetString("UserId", result.User.Id.ToString());
```

## Error Handling

### Session Not Available
```csharp
if (session == null)
{
    _logger.LogWarning("Session is not available");
    return;
}
```

### Exception Handling
```csharp
try
{
    // Session operations
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error setting user session");
    throw;
}
```

## Logging

### Information Logs
```csharp
_logger.LogInformation("User session set for user: {UserName} ({Email})", 
    result.User.UserName, result.User.Email);
```

### Debug Logs
```csharp
_logger.LogDebug("JWT token stored in session");
```

### Warning Logs
```csharp
_logger.LogWarning("Session is not available");
```

### Error Logs
```csharp
_logger.LogError(ex, "Error setting user session");
```

## Best Practices

### ✅ Null Safety
```csharp
var session = _httpContextAccessor.HttpContext?.Session;
if (session == null)
{
    _logger.LogWarning("Session is not available");
    return;
}
```

### ✅ Validation
```csharp
if (!string.IsNullOrEmpty(result.AccessToken))
{
    session.SetString("JWTToken", result.AccessToken);
}
```

### ✅ Logging
```csharp
_logger.LogInformation("User session set for user: {UserName} ({Email})", 
    result.User.UserName, result.User.Email);
```

### ✅ Exception Handling
```csharp
try
{
    // Session operations
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error setting user session");
    throw;
}
```

## Testing

### Unit Testing
```csharp
[Test]
public void SetUserSession_ValidData_SetsSessionCorrectly()
{
    // Arrange
    var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
    var mockSession = new Mock<ISession>();
    var mockLogger = new Mock<ILogger<UserSessionService>>();
    
    mockHttpContextAccessor.Setup(x => x.HttpContext.Session).Returns(mockSession.Object);
    
    var service = new UserSessionService(mockHttpContextAccessor.Object, mockLogger.Object);
    var authResult = new AuthResultDto 
    { 
        AccessToken = "token",
        User = new UserDto { UserName = "test", Email = "test@example.com" }
    };
    
    // Act
    service.SetUserSession(authResult);
    
    // Assert
    mockSession.Verify(x => x.SetString("JWTToken", "token"), Times.Once);
    mockSession.Verify(x => x.SetString("UserName", "test"), Times.Once);
}
```

### Integration Testing
```csharp
[Test]
public void SetUserSession_WithRealSession_WorksCorrectly()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddSession();
    services.AddHttpContextAccessor();
    services.AddScoped<IUserSessionService, UserSessionService>();
    
    var serviceProvider = services.BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IUserSessionService>();
    
    var authResult = new AuthResultDto 
    { 
        AccessToken = "token",
        User = new UserDto { UserName = "test", Email = "test@example.com" }
    };
    
    // Act
    service.SetUserSession(authResult);
    
    // Assert
    Assert.AreEqual("test", service.GetUserName());
    Assert.AreEqual("token", service.GetJwtToken());
}
```

## Future Enhancements

### Session Expiration
```csharp
public void SetUserSession(AuthResultDto result, TimeSpan? expiration = null)
{
    // Set session with custom expiration
}
```

### Session Encryption
```csharp
public void SetUserSession(AuthResultDto result, bool encrypt = true)
{
    // Encrypt sensitive session data
}
```

### Session Validation
```csharp
public bool ValidateSession()
{
    // Validate session integrity
}
```

### Session Refresh
```csharp
public void RefreshSession()
{
    // Refresh session timeout
}
```

## Troubleshooting

### Common Issues

1. **Session Not Available**
   - اطمینان از ثبت `AddHttpContextAccessor()`
   - بررسی middleware order

2. **Session Data Lost**
   - بررسی session timeout
   - بررسی session storage configuration

3. **Null Reference Exceptions**
   - استفاده از null-safe operators
   - بررسی HttpContext availability

### Debug Tips

- بررسی logs برای session operations
- استفاده از browser developer tools
- بررسی session data در memory
- تست session در isolation 