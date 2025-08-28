# راهنمای کامل Typed HTTP Client با Authentication

## 🎯 **هدف**

این سیستم برای حل مشکلات زیر طراحی شده:

1. **عدم وجود Authentication Interceptor** - JWT token به صورت خودکار اضافه نمی‌شد
2. **عدم مدیریت خودکار Token** - هر بار باید دستی token رو از session بگیریم
3. **عدم وجود Centralized HTTP Client** - تکرار کد در همه جا
4. **عدم Error Handling مناسب** - 401/403 errors درست handle نمی‌شدند

## 🏗️ **معماری جدید**

### **1. AuthenticationInterceptor**
```csharp
// مسئولیت: مدیریت خودکار JWT tokens
- اضافه کردن Authorization header
- بررسی اعتبار token
- Refresh کردن token در صورت نیاز
- Handle کردن 401/403 errors
```

### **2. AuthenticatedHttpClient**
```csharp
// مسئولیت: HTTP client با authentication خودکار
- GET, POST, PUT, DELETE, PATCH methods
- اضافه کردن خودکار JWT token
- Retry کردن درخواست بعد از token refresh
- Error handling مرکزی
```

### **3. Typed API Clients**
```csharp
// مسئولیت: API calls برای domain های مختلف
- AuthApiClient: برای authentication
- UserApiClient: برای user management
- ProductApiClient: برای product management
- و غیره...
```

## 🚀 **نحوه استفاده**

### **1. استفاده از AuthApiClient (مثال)**
```csharp
public class AuthController : BaseController
{
    private readonly IAuthApiClient _authApiClient;

    public AuthController(IAuthApiClient authApiClient, IUserSessionService userSessionService, ILogger<AuthController> logger)
        : base(userSessionService, logger)
    {
        _authApiClient = authApiClient;
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        // ✅ ساده و تمیز - همه چیز خودکار handle می‌شه
        var result = await _authApiClient.LoginAsync(loginDto);
        
        if (result?.IsSuccess == true)
        {
            UserSessionService.SetUserSession(result);
            return RedirectToAction("Index", "Home");
        }
        
        ModelState.AddModelError("", result?.ErrorMessage ?? "Login failed");
        return View(loginDto);
    }
}
```

### **2. استفاده از UserApiClient (مثال)**
```csharp
public class UserController : BaseController
{
    private readonly IUserApiClient _userApiClient;

    public UserController(IUserApiClient userApiClient, IUserSessionService userSessionService, ILogger<UserController> logger)
        : base(userSessionService, logger)
    {
        _userApiClient = userApiClient;
    }

    public async Task<IActionResult> Profile()
    {
        // ✅ JWT token خودکار اضافه می‌شه
        // ✅ اگر token expire شده باشه، خودکار refresh می‌شه
        // ✅ اگر 401 بیاد، خودکار retry می‌شه
        var profile = await _userApiClient.GetUserProfileAsync();
        
        if (profile == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(UpdateUserDto updateDto)
    {
        // ✅ همه چیز خودکار handle می‌شه
        var result = await _userApiClient.UpdateUserAsync(CurrentUserId!, updateDto);
        
        if (result != null)
        {
            TempData["Success"] = "Profile updated successfully";
            return RedirectToAction("Profile");
        }
        
        ModelState.AddModelError("", "Failed to update profile");
        return View(updateDto);
    }
}
```

### **3. ساخت API Client جدید**
```csharp
// 1. Interface بسازید
public interface IProductApiClient
{
    Task<ProductDto?> GetProductAsync(int productId);
    Task<PaginatedResult<ProductDto>?> GetProductsAsync(int page = 1, int pageSize = 10);
    Task<ProductDto?> CreateProductAsync(CreateProductDto createDto);
    Task<ProductDto?> UpdateProductAsync(int productId, UpdateProductDto updateDto);
    Task<bool> DeleteProductAsync(int productId);
}

// 2. Implementation بسازید
public class ProductApiClient : IProductApiClient
{
    private readonly IAuthenticatedHttpClient _httpClient;
    private readonly ILogger<ProductApiClient> _logger;

    public ProductApiClient(IAuthenticatedHttpClient httpClient, ILogger<ProductApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductAsync(int productId)
    {
        try
        {
            _logger.LogDebug("Getting product: {ProductId}", productId);
            
            // ✅ همه چیز خودکار handle می‌شه
            var result = await _httpClient.GetAsync<ProductDto>($"api/Product/{productId}");
            
            if (result != null)
            {
                _logger.LogDebug("Product retrieved successfully: {ProductId}", productId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product: {ProductId}", productId);
            return null;
        }
    }

    // سایر methods...
}

// 3. در Program.cs register کنید
builder.Services.AddScoped<IProductApiClient, ProductApiClient>();
```

## 🔧 **تنظیمات**

### **Program.cs**
```csharp
// Add Typed HttpClient for API communication
builder.Services.AddHttpClient<HttpClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7209/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register authentication interceptor
builder.Services.AddScoped<IAuthenticationInterceptor, AuthenticationInterceptor>();

// Register authenticated HTTP client
builder.Services.AddScoped<IAuthenticatedHttpClient, AuthenticatedHttpClient>();

// Register session management service
builder.Services.AddScoped<IUserSessionService, UserSessionService>();

// Register API clients
builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<IUserApiClient, UserApiClient>();
builder.Services.AddScoped<IProductApiClient, ProductApiClient>();
```

## 🛡️ **امنیت**

### **1. Automatic Token Management**
- ✅ JWT token خودکار به همه درخواست‌ها اضافه می‌شه
- ✅ Token expiration خودکار check می‌شه
- ✅ Token refresh خودکار انجام می‌شه
- ✅ Session cleanup در صورت token invalidation

### **2. Error Handling**
- ✅ 401 Unauthorized: خودکار token refresh و retry
- ✅ 403 Forbidden: Log و return false
- ✅ Network errors: Proper logging و graceful handling
- ✅ Timeout errors: Configurable timeout

### **3. Security Headers**
```csharp
// Custom headers اضافه کردن
_httpClient.AddHeader("X-Custom-Header", "value");
_httpClient.AddHeader("X-API-Version", "v1");

// Headers پاک کردن
_httpClient.ClearHeaders();
```

## 📊 **Logging**

### **AuthenticationInterceptor Logs**
```
[Debug] Authentication header added to request: GET /api/User/profile
[Debug] Token is expired or expiring soon. Expires: 2024-01-15T10:30:00Z, Current: 2024-01-15T10:25:00Z
[Information] Attempting to refresh access token
[Information] Token refreshed successfully
[Warning] Received 401 Unauthorized, attempting token refresh
[Information] Token refreshed successfully after 401 error
```

### **AuthenticatedHttpClient Logs**
```
[Debug] Sending GET request to: /api/User/profile
[Debug] Successfully deserialized response to UserProfileDto
[Warning] Request failed with status 403: Insufficient permissions
[Error] Error sending GET request to: /api/User/profile
```

## 🔄 **Token Refresh Flow**

### **1. Normal Request**
```
Client → AuthenticatedHttpClient → AuthenticationInterceptor → API
                                    ↓
                              Check Token Valid
                                    ↓
                              Add Authorization Header
                                    ↓
                              Send Request
```

### **2. Token Expired**
```
Client → AuthenticatedHttpClient → AuthenticationInterceptor → API
                                    ↓
                              Check Token Valid (False)
                                    ↓
                              Refresh Token
                                    ↓
                              Update Session
                                    ↓
                              Add New Authorization Header
                                    ↓
                              Send Request
```

### **3. 401 Response**
```
API → 401 Unauthorized → AuthenticatedHttpClient
                              ↓
                        Handle Authentication Error
                              ↓
                        Refresh Token
                              ↓
                        Retry Request with New Token
```

## 🎯 **مزایا**

### **1. برای Developer**
- ✅ **سادگی**: فقط endpoint و data بدهید
- ✅ **Type Safety**: Strongly-typed responses
- ✅ **IntelliSense**: کامل support
- ✅ **Error Handling**: مرکزی و خودکار

### **2. برای Security**
- ✅ **Automatic Authentication**: JWT خودکار اضافه می‌شه
- ✅ **Token Refresh**: خودکار و transparent
- ✅ **Session Management**: امن و مرکزی
- ✅ **Error Handling**: مناسب برای security events

### **3. برای Maintenance**
- ✅ **DRY Principle**: تکرار کد حذف شده
- ✅ **Centralized Logic**: همه چیز در یک جا
- ✅ **Easy Testing**: Mock کردن ساده
- ✅ **Consistent Behavior**: همه API calls یکسان

## 🚨 **نکات مهم**

### **1. Session Management**
```csharp
// ✅ درست: از UserSessionService استفاده کنید
UserSessionService.SetUserSession(authResult);
var token = UserSessionService.GetJwtToken();

// ❌ اشتباه: مستقیماً session access کنید
HttpContext.Session.SetString("JWTToken", token);
```

### **2. Error Handling**
```csharp
// ✅ درست: null check کنید
var result = await _userApiClient.GetUserProfileAsync();
if (result == null)
{
    // Handle error
    return RedirectToAction("Login", "Auth");
}

// ❌ اشتباه: بدون null check
var result = await _userApiClient.GetUserProfileAsync();
return View(result); // ممکنه null باشه
```

### **3. Logging**
```csharp
// ✅ درست: از structured logging استفاده کنید
_logger.LogInformation("User {UserId} updated successfully", userId);

// ❌ اشتباه: string concatenation
_logger.LogInformation("User " + userId + " updated successfully");
```

## 🔧 **Troubleshooting**

### **1. Token Not Being Added**
- ✅ `IAuthenticationInterceptor` register شده؟
- ✅ `IUserSessionService` درست کار می‌کنه؟
- ✅ Session data موجوده؟

### **2. 401 Errors**
- ✅ Token valid هست؟
- ✅ Refresh token موجوده؟
- ✅ API endpoint درست هست؟

### **3. Timeout Errors**
- ✅ Network connection درست هست؟
- ✅ API server در دسترس هست؟
- ✅ Timeout setting مناسب هست؟

## 📚 **References**

- [ASP.NET Core HTTP Client Factory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)
- [JWT Best Practices](https://auth0.com/blog/a-look-at-the-latest-draft-for-jwt-bcp/)
- [HTTP Client Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) 