# راهنمای حل مشکل Circular Dependency

## 🎯 **مشکل**

Circular Dependency در dependency injection:

```
JwtAuthenticationHandler -> UserSessionService -> AuthApiClient -> AuthenticatedHttpClient -> AuthenticationInterceptor -> UserSessionService
```

## 🔍 **تحلیل مشکل**

### **Dependency Chain:**
1. `JwtAuthenticationHandler` → `IUserSessionService`
2. `UserSessionService` → `IAuthApiClient`
3. `AuthApiClient` → `IAuthenticatedHttpClient`
4. `AuthenticatedHttpClient` → `IAuthenticationInterceptor`
5. `AuthenticationInterceptor` → `IUserSessionService` ← **Circular!**

## ✅ **راه حل**

### **1. حذف Dependency از UserSessionService**

**قبل:**
```csharp
// ❌ UserSessionService به IAuthApiClient وابسته بود
public class UserSessionService : IUserSessionService
{
    private readonly IAuthApiClient _authApiClient;
    
    public UserSessionService(
        IHttpContextAccessor httpContextAccessor, 
        ILogger<UserSessionService> logger,
        IAuthApiClient authApiClient, // ❌ این dependency مشکل‌ساز بود
        IConfiguration configuration)
    {
        _authApiClient = authApiClient;
    }
}
```

**بعد:**
```csharp
// ✅ UserSessionService از IHttpClientFactory استفاده می‌کند
public class UserSessionService : IUserSessionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public UserSessionService(
        IHttpContextAccessor httpContextAccessor, 
        ILogger<UserSessionService> logger,
        IHttpClientFactory httpClientFactory, // ✅ جایگزین شده
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
    }
}
```

### **2. استفاده از HttpClientFactory**

**قبل:**
```csharp
// ❌ استفاده مستقیم از IAuthApiClient
private async Task InvalidateRefreshTokenOnBackendAsync(CancellationToken cancellationToken = default)
{
    var logoutDto = GetLogoutDto();
    var result = await _authApiClient.LogoutAsync(logoutDto, cancellationToken);
    
    if (result.IsSuccess)
    {
        _logger.LogDebug("Refresh token invalidated on backend");
    }
}
```

**بعد:**
```csharp
// ✅ استفاده از HttpClientFactory
private async Task InvalidateRefreshTokenOnBackendAsync(CancellationToken cancellationToken = default)
{
    var logoutDto = GetLogoutDto();
    
    // Use HttpClientFactory to make direct API call
    var httpClient = _httpClientFactory.CreateClient("ApiClient");
    var json = JsonSerializer.Serialize(logoutDto, _jsonOptions);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    
    var response = await httpClient.PostAsync("api/Auth/logout", content, cancellationToken);
    
    if (response.IsSuccessStatusCode)
    {
        _logger.LogDebug("Refresh token invalidated on backend");
    }
    else
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning("Failed to invalidate refresh token on backend. Status: {StatusCode}, Error: {Error}", 
            response.StatusCode, errorContent);
    }
}
```

### **3. LogoutAsync Method Update**

**قبل:**
```csharp
// ❌ استفاده مستقیم از IAuthApiClient
public async Task<ApiResponse<LogoutResultDto>> LogoutAsync(bool logoutFromAllDevices = false, CancellationToken cancellationToken = default)
{
    var logoutDto = new LogoutDto
    {
        RefreshToken = refreshToken,
        LogoutFromAllDevices = logoutFromAllDevices
    };

    backendResult = await _authApiClient.LogoutAsync(logoutDto, cancellationToken);
}
```

**بعد:**
```csharp
// ✅ استفاده از HttpClientFactory
public async Task<ApiResponse<LogoutResultDto>> LogoutAsync(bool logoutFromAllDevices = false, CancellationToken cancellationToken = default)
{
    var logoutDto = new LogoutDto
    {
        RefreshToken = refreshToken,
        LogoutFromAllDevices = logoutFromAllDevices
    };

    // Use HttpClientFactory to make direct API call
    var httpClient = _httpClientFactory.CreateClient("ApiClient");
    var json = JsonSerializer.Serialize(logoutDto, _jsonOptions);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    
    var response = await httpClient.PostAsync("api/Auth/logout", content, cancellationToken);
    
    if (response.IsSuccessStatusCode)
    {
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<LogoutResultDto>(responseContent, _jsonOptions);
        backendResult = ApiResponse<LogoutResultDto>.Success(result ?? new LogoutResultDto());
    }
    else
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        backendResult = ApiResponse<LogoutResultDto>.Error($"Logout failed: {errorContent}", (int)response.StatusCode);
    }
}
```

## 🏗️ **معماری جدید**

### **Dependency Chain جدید:**
```
JwtAuthenticationHandler -> UserSessionService -> HttpClientFactory
AuthApiClient -> AuthenticatedHttpClient -> AuthenticationInterceptor -> UserSessionService
```

### **جداسازی مسئولیت‌ها:**

1. **UserSessionService**: مدیریت session و cookies
2. **HttpClientFactory**: ارتباط مستقیم با API
3. **AuthApiClient**: لایه بالاتر برای authentication
4. **AuthenticatedHttpClient**: HTTP client با authentication
5. **AuthenticationInterceptor**: مدیریت automatic token refresh

## 📊 **مزایای راه حل**

### **1. حذف Circular Dependency**
```csharp
// ✅ No more circular dependency
// UserSessionService -> HttpClientFactory (not AuthApiClient)
// AuthApiClient -> AuthenticatedHttpClient -> AuthenticationInterceptor -> UserSessionService
```

### **2. جداسازی مسئولیت‌ها**
```csharp
// ✅ Clear separation of concerns
// UserSessionService: Session management
// HttpClientFactory: Direct API communication
// AuthApiClient: High-level authentication operations
```

### **3. انعطاف‌پذیری بیشتر**
```csharp
// ✅ More flexibility
// UserSessionService can make direct API calls when needed
// No dependency on high-level services for basic operations
```

### **4. Performance بهتر**
```csharp
// ✅ Better performance
// Direct HTTP calls without going through multiple layers
// Reduced overhead for simple operations
```

## 🔧 **Implementation Details**

### **1. Constructor Update**
```csharp
public UserSessionService(
    IHttpContextAccessor httpContextAccessor, 
    ILogger<UserSessionService> logger,
    IHttpClientFactory httpClientFactory, // ✅ جایگزین IAuthApiClient
    IConfiguration configuration)
{
    _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    _cookieConfig = CookieSecurityConfig.FromConfiguration(configuration);
    
    _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
```

### **2. Direct API Calls**
```csharp
// ✅ Direct API calls using HttpClientFactory
private async Task<HttpResponseMessage> MakeApiCallAsync(string endpoint, object data, CancellationToken cancellationToken = default)
{
    var httpClient = _httpClientFactory.CreateClient("ApiClient");
    var json = JsonSerializer.Serialize(data, _jsonOptions);
    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    
    return await httpClient.PostAsync(endpoint, content, cancellationToken);
}
```

### **3. Error Handling**
```csharp
// ✅ Proper error handling for direct API calls
try
{
    var response = await MakeApiCallAsync("api/Auth/logout", logoutDto, cancellationToken);
    
    if (response.IsSuccessStatusCode)
    {
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<LogoutResultDto>(responseContent, _jsonOptions);
        return ApiResponse<LogoutResultDto>.Success(result ?? new LogoutResultDto());
    }
    else
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return ApiResponse<LogoutResultDto>.Error($"Logout failed: {errorContent}", (int)response.StatusCode);
    }
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error during logout");
    return ApiResponse<LogoutResultDto>.Error("An error occurred during logout", 500);
}
```

## ✅ **Best Practices**

### **1. استفاده از HttpClientFactory**
```csharp
// ✅ Always use HttpClientFactory for HTTP calls
// Avoid creating HttpClient instances directly
// Proper connection pooling and lifecycle management
```

### **2. جداسازی مسئولیت‌ها**
```csharp
// ✅ Keep services focused on their primary responsibility
// UserSessionService: Session management
// AuthApiClient: Authentication operations
// Avoid mixing concerns
```

### **3. Error Handling**
```csharp
// ✅ Proper error handling for all API calls
// Log errors appropriately
// Return meaningful error responses
```

### **4. CancellationToken Support**
```csharp
// ✅ Always support CancellationToken
// Proper cancellation handling
// Resource cleanup
```

## 🔄 **Migration Steps**

### **1. شناسایی Circular Dependencies**
```csharp
// ✅ Analyze dependency chain
// Identify circular references
// Plan breaking points
```

### **2. حذف Dependencies غیرضروری**
```csharp
// ✅ Remove unnecessary dependencies
// Use lower-level services when possible
// Break circular chains
```

### **3. استفاده از HttpClientFactory**
```csharp
// ✅ Replace high-level service dependencies
// Use HttpClientFactory for direct API calls
// Maintain functionality while breaking cycles
```

### **4. تست و Validation**
```csharp
// ✅ Test all functionality
// Ensure no regressions
// Validate performance improvements
```

## 🛡️ **Security Considerations**

### **1. Direct API Calls**
```csharp
// ✅ Secure direct API calls
// Proper authentication headers
// Error handling for security-related operations
```

### **2. Token Management**
```csharp
// ✅ Maintain secure token handling
// No compromise in security
// Proper token validation
```

### **3. Error Information**
```csharp
// ✅ Secure error information
// Don't expose sensitive data in logs
// Proper error sanitization
```

## 🔍 **Debugging و Monitoring**

### **1. Dependency Analysis**
```csharp
// ✅ Monitor dependency resolution
// Track service construction
// Identify potential circular dependencies early
```

### **2. Performance Monitoring**
```csharp
// ✅ Monitor API call performance
// Track direct vs indirect calls
// Measure improvements
```

### **3. Error Tracking**
```csharp
// ✅ Track API call errors
// Monitor error rates
// Identify patterns
```

این راه حل تضمین می‌کند که:
- ✅ Circular Dependency حل شود
- ✅ عملکرد حفظ شود
- ✅ امنیت حفظ شود
- ✅ انعطاف‌پذیری افزایش یابد
- ✅ مسئولیت‌ها جدا شوند 