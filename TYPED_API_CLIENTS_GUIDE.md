# راهنمای Typed API Clients

## خلاصه

این راهنما نحوه استفاده از Typed API Clients برای ارتباط type-safe با Backend API را توضیح می‌دهد.

## ساختار

### Interface
```csharp
public interface IAuthApiClient
{
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto> LoginAsync(LoginDto dto);
    Task<bool> LogoutAsync();
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync();
}
```

### Implementation
```csharp
public class AuthApiClient : IAuthApiClient
{
    private readonly IExternalService _externalService;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(IExternalService externalService, ILogger<AuthApiClient> logger)
    {
        _externalService = externalService;
        _logger = logger;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        // Implementation with error handling and logging
    }
}
```

## نحوه استفاده

### در Controller
```csharp
public class AuthController : Controller
{
    private readonly IAuthApiClient _authApiClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthApiClient authApiClient, ILogger<AuthController> logger)
    {
        _authApiClient = authApiClient;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _authApiClient.RegisterAsync(model);
            
            if (response.IsSuccess)
            {
                // Handle success
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", response.ErrorMessage);
                return View(model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            ModelState.AddModelError("", "خطا در ارتباط با سرور");
            return View(model);
        }
    }
}
```

### در Program.cs
```csharp
// Register typed API clients
builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
```

## مزایا

### ✅ Dependency Inversion
- Controller دیگر route API را نمی‌داند
- وابستگی به abstraction نه implementation
- تست‌پذیری بهتر

### ✅ Separation of Concerns
- جداسازی منطق API communication از Controller
- مسئولیت‌های مشخص و جداگانه
- کد تمیزتر و قابل نگهداری‌تر

### ✅ Type Safety
- استفاده از DTO های موجود
- Compile-time error detection
- IntelliSense support بهتر

### ✅ Error Handling
- مدیریت خطا در یک مکان
- Logging متمرکز
- Error messages یکپارچه

### ✅ Reusability
- امکان استفاده مجدد در جاهای دیگر
- کاهش duplication
- مدیریت متمرکز API calls

## ایجاد API Client جدید

### 1. ایجاد Interface
```csharp
public interface IUserApiClient
{
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(Guid id);
}
```

### 2. ایجاد Implementation
```csharp
public class UserApiClient : IUserApiClient
{
    private readonly IExternalService _externalService;
    private readonly ILogger<UserApiClient> _logger;

    public UserApiClient(IExternalService externalService, ILogger<UserApiClient> logger)
    {
        _externalService = externalService;
        _logger = logger;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting user by ID: {UserId}", id);
            
            var result = await _externalService.GetAsync<UserDto>($"api/Users/{id}");
            
            _logger.LogInformation("Successfully retrieved user: {UserId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            throw;
        }
    }

    // Other methods...
}
```

### 3. ثبت در DI Container
```csharp
builder.Services.AddScoped<IUserApiClient, UserApiClient>();
```

### 4. استفاده در Controller
```csharp
public class UserController : Controller
{
    private readonly IUserApiClient _userApiClient;

    public UserController(IUserApiClient userApiClient)
    {
        _userApiClient = userApiClient;
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var user = await _userApiClient.GetUserByIdAsync(id);
        return View(user);
    }
}
```

## Best Practices

### ✅ Error Handling
```csharp
public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
{
    try
    {
        var result = await _externalService.PostAsync<RegisterDto, AuthResultDto>("api/Auth/register", dto);
        
        if (result?.IsSuccess == true)
        {
            _logger.LogInformation("Registration successful for: {Email}", dto.Email);
        }
        else
        {
            _logger.LogWarning("Registration failed for: {Email}. Error: {Error}", 
                dto.Email, result?.ErrorMessage);
        }
        
        return result ?? new AuthResultDto { IsSuccess = false, ErrorMessage = "No response from server" };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during registration for: {Email}", dto.Email);
        return new AuthResultDto { IsSuccess = false, ErrorMessage = "خطا در ارتباط با سرور" };
    }
}
```

### ✅ Logging
- استفاده از structured logging
- Log کردن اطلاعات مهم
- Log کردن errors با جزئیات

### ✅ Null Safety
- بررسی null برای responses
- استفاده از null-coalescing operator
- Return کردن default values در صورت خطا

### ✅ Async/Await
- استفاده صحیح از async/await
- عدم استفاده از .Result یا .Wait()
- Proper exception handling

## Testing

### Unit Testing
```csharp
[Test]
public async Task RegisterAsync_ValidData_ReturnsSuccess()
{
    // Arrange
    var mockExternalService = new Mock<IExternalService>();
    var mockLogger = new Mock<ILogger<AuthApiClient>>();
    var authClient = new AuthApiClient(mockExternalService.Object, mockLogger.Object);
    
    var registerDto = new RegisterDto { Email = "test@example.com" };
    var expectedResult = new AuthResultDto { IsSuccess = true };
    
    mockExternalService.Setup(x => x.PostAsync<RegisterDto, AuthResultDto>(
        It.IsAny<string>(), It.IsAny<RegisterDto>()))
        .ReturnsAsync(expectedResult);
    
    // Act
    var result = await authClient.RegisterAsync(registerDto);
    
    // Assert
    Assert.IsTrue(result.IsSuccess);
    mockExternalService.Verify(x => x.PostAsync<RegisterDto, AuthResultDto>(
        "api/Auth/register", registerDto), Times.Once);
}
```

### Integration Testing
```csharp
[Test]
public async Task RegisterAsync_WithRealApi_ReturnsSuccess()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<IAuthApiClient, AuthApiClient>();
    services.AddScoped<IExternalService, ExternalService>();
    // ... other dependencies
    
    var serviceProvider = services.BuildServiceProvider();
    var authClient = serviceProvider.GetRequiredService<IAuthApiClient>();
    
    var registerDto = new RegisterDto 
    { 
        Email = "test@example.com",
        UserName = "testuser",
        Password = "password123",
        ConfirmPassword = "password123",
        AcceptTerms = true
    };
    
    // Act
    var result = await authClient.RegisterAsync(registerDto);
    
    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.IsNotNull(result.AccessToken);
    Assert.IsNotNull(result.User);
}
```

## Future Enhancements

### Caching
```csharp
public class CachedAuthApiClient : IAuthApiClient
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IMemoryCache _cache;
    
    public async Task<AuthResultDto> GetUserProfileAsync(Guid userId)
    {
        var cacheKey = $"user_profile_{userId}";
        
        if (_cache.TryGetValue(cacheKey, out UserDto cachedUser))
        {
            return cachedUser;
        }
        
        var result = await _authApiClient.GetUserProfileAsync(userId);
        
        if (result.IsSuccess)
        {
            _cache.Set(cacheKey, result.User, TimeSpan.FromMinutes(30));
        }
        
        return result;
    }
}
```

### Retry Logic
```csharp
public class RetryAuthApiClient : IAuthApiClient
{
    private readonly IAuthApiClient _authApiClient;
    private readonly ILogger<RetryAuthApiClient> _logger;
    
    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        var retryPolicy = Policy<AuthResultDto>
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} after {Delay}ms", 
                        retryCount, timeSpan.TotalMilliseconds);
                });
        
        return await retryPolicy.ExecuteAsync(() => _authApiClient.RegisterAsync(dto));
    }
}
```

### Circuit Breaker
```csharp
public class CircuitBreakerAuthApiClient : IAuthApiClient
{
    private readonly IAuthApiClient _authApiClient;
    private readonly ILogger<CircuitBreakerAuthApiClient> _logger;
    
    private readonly AsyncCircuitBreakerPolicy<AuthResultDto> _circuitBreakerPolicy;
    
    public CircuitBreakerAuthApiClient(IAuthApiClient authApiClient, ILogger<CircuitBreakerAuthApiClient> logger)
    {
        _authApiClient = authApiClient;
        _logger = logger;
        
        _circuitBreakerPolicy = Policy<AuthResultDto>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    _logger.LogWarning("Circuit breaker opened for {Duration}ms", duration.TotalMilliseconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset");
                });
    }
    
    public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
    {
        return await _circuitBreakerPolicy.ExecuteAsync(() => _authApiClient.RegisterAsync(dto));
    }
}
``` 