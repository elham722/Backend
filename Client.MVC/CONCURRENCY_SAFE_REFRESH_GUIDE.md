# راهنمای Concurrency-Safe Refresh Token

## مشکل اصلی

در سیستم‌های احراز هویت با JWT، وقتی چندین درخواست همزمان می‌آیند و توکن منقضی شده باشد، همهٔ آن‌ها همزمان تلاش می‌کنند refresh کنند. این باعث مشکلات زیر می‌شود:

1. **RefreshToken یکبار مصرف**: ممکن است RefreshToken یکبار مصرف شود
2. **تداخل درخواست‌ها**: درخواست‌های همزمان ممکن است تداخل کنند
3. **Race Condition**: وضعیت‌های ناسازگار در سیستم
4. **عملکرد ضعیف**: درخواست‌های اضافی و غیرضروری

## راه‌حل پیاده‌سازی شده

### 1. AuthenticationInterceptor بهبود یافته

#### ویژگی‌های کلیدی:
- **SemaphoreSlim**: قفل کردن عملیات refresh
- **Task Sharing**: اشتراک‌گذاری نتیجه refresh بین درخواست‌های همزمان
- **State Management**: مدیریت وضعیت refresh با thread-safety
- **Timeout Handling**: مدیریت timeout برای جلوگیری از deadlock

#### کد کلیدی:
```csharp
private static readonly SemaphoreSlim _refreshLock = new(1, 1);
private static Task<bool>? _currentRefreshTask = null;

public async Task<bool> RefreshTokenIfNeededAsync()
{
    // Check if already refreshing
    if (_isRefreshing && _currentRefreshTask != null)
    {
        return await _currentRefreshTask; // Share existing task
    }

    // Acquire lock
    await _refreshLock.WaitAsync(_refreshTimeout);
    try
    {
        // Double-check pattern
        if (_isRefreshing && _currentRefreshTask != null)
        {
            return await _currentRefreshTask;
        }
        
        _isRefreshing = true;
        _currentRefreshTask = PerformRefreshAsync();
        return await _currentRefreshTask;
    }
    finally
    {
        _refreshLock.Release();
    }
}
```

### 2. ConcurrencyManager پیشرفته

#### قابلیت‌ها:
- **Operation Locking**: قفل کردن عملیات بر اساس کلید
- **Task Sharing**: اشتراک‌گذاری نتیجه عملیات
- **Timeout Management**: مدیریت timeout
- **Cancellation Support**: پشتیبانی از لغو عملیات
- **Statistics**: آمارگیری از عملیات

#### استفاده:
```csharp
// Execute with lock
var result = await _concurrencyManager.ExecuteWithLockAsync(
    "token-refresh", 
    async () => await RefreshTokenOperation(),
    TimeSpan.FromSeconds(30)
);

// Check if operation in progress
if (_concurrencyManager.IsOperationInProgress("token-refresh"))
{
    // Wait for completion
    await _concurrencyManager.WaitForOperationCompletionAsync("token-refresh");
}
```

### 3. TokenManager تخصصی

#### ویژگی‌ها:
- **Concurrency-Safe Refresh**: استفاده از ConcurrencyManager
- **Token Validation**: اعتبارسنجی توکن
- **Expiration Checking**: بررسی انقضای توکن
- **Caching Integration**: یکپارچه‌سازی با کش

#### استفاده:
```csharp
// Refresh if needed
var success = await _tokenManager.RefreshTokenIfNeededAsync();

// Check if token is expiring soon
if (_tokenManager.IsTokenExpiringSoon(TimeSpan.FromMinutes(10)))
{
    await _tokenManager.RefreshTokenAsync();
}
```

## مزایای راه‌حل

### 1. امنیت
- **جلوگیری از Race Condition**: فقط یک refresh در هر زمان
- **محافظت از RefreshToken**: جلوگیری از استفاده چندباره
- **State Consistency**: حفظ سازگاری وضعیت

### 2. عملکرد
- **Task Sharing**: اشتراک‌گذاری نتیجه بین درخواست‌های همزمان
- **Reduced Network Calls**: کاهش درخواست‌های شبکه
- **Efficient Locking**: قفل‌گذاری کارآمد

### 3. قابلیت نگهداری
- **Separation of Concerns**: جداسازی مسئولیت‌ها
- **Reusable Components**: کامپوننت‌های قابل استفاده مجدد
- **Comprehensive Logging**: لاگینگ کامل

## نحوه استفاده

### 1. در AuthenticationInterceptor

```csharp
public async Task<HttpRequestMessage> AddAuthenticationHeaderAsync(HttpRequestMessage request)
{
    // Check if token is valid and refresh if needed
    if (!IsTokenValid())
    {
        var refreshSuccess = await RefreshTokenIfNeededAsync();
        if (!refreshSuccess)
        {
            return request; // Proceed without authentication
        }
    }

    var token = GetCurrentToken();
    if (!string.IsNullOrEmpty(token))
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    return request;
}
```

### 2. در AuthenticatedHttpClient

```csharp
public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint)
{
    var httpClient = _httpClientFactory.CreateClient("ApiClient");
    var policy = _resiliencePolicyService.CreateReadOnlyPolicy();
    
    var response = await policy.ExecuteAsync(async (context) =>
    {
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        
        // Add authentication header (handles refresh automatically)
        request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
        
        var httpResponse = await httpClient.SendAsync(request);
        
        // Handle authentication errors
        if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var handled = await _authInterceptor.HandleAuthenticationErrorAsync(httpResponse);
            if (handled)
            {
                // Retry with new token
                request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
                httpResponse = await httpClient.SendAsync(request);
            }
        }
        
        return httpResponse;
    });
    
    return await ProcessResponseAsync<TResponse>(response);
}
```

### 3. در Controller

```csharp
public class SomeController : BaseController
{
    private readonly ITokenManager _tokenManager;

    public SomeController(ITokenManager tokenManager, ...)
    {
        _tokenManager = tokenManager;
    }

    public async Task<IActionResult> SomeAction()
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            // Token refresh is handled automatically by AuthenticationInterceptor
            var result = await _someService.GetDataAsync();
            return View(result);
        }, "SomeAction");
    }
}
```

## تنظیمات پیشنهادی

### 1. Timeout Settings

```json
{
  "TokenSettings": {
    "RefreshTimeoutSeconds": 30,
    "RefreshCooldownSeconds": 5,
    "ExpirationThresholdMinutes": 5
  }
}
```

### 2. Resilience Settings

```json
{
  "ResiliencePolicies": {
    "Auth": {
      "RetryCount": 3,
      "RetryDelaySeconds": 2,
      "CircuitBreakerThreshold": 3,
      "CircuitBreakerDurationSeconds": 30,
      "TimeoutSeconds": 10
    }
  }
}
```

## تست و مانیتورینگ

### 1. تست همزمانی

```csharp
[Test]
public async Task ConcurrentRefreshRequests_ShouldShareResult()
{
    // Arrange
    var tasks = new List<Task<bool>>();
    
    // Act - Start multiple refresh requests simultaneously
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(_tokenManager.RefreshTokenAsync());
    }
    
    var results = await Task.WhenAll(tasks);
    
    // Assert - All should return the same result
    Assert.That(results.All(r => r == results[0]), Is.True);
}
```

### 2. مانیتورینگ

```csharp
// Get concurrency statistics
var stats = _concurrencyManager.GetStatistics();
_logger.LogInformation("Concurrency Stats: {Stats}", stats);

// Monitor token refresh operations
if (_tokenManager.IsTokenExpiringSoon())
{
    _logger.LogWarning("Token is expiring soon, refresh may be needed");
}
```

## نکات مهم

1. **Timeout Management**: همیشه timeout مناسب تنظیم کنید
2. **Error Handling**: خطاها را به درستی مدیریت کنید
3. **Logging**: لاگینگ کامل برای دیباگ
4. **Testing**: تست همزمانی انجام دهید
5. **Monitoring**: مانیتورینگ عملکرد

## نتیجه‌گیری

این راه‌حل مشکل همزمانی refresh token را به طور کامل حل می‌کند و مزایای زیر را فراهم می‌کند:

- ✅ **امنیت کامل**: جلوگیری از race condition
- ✅ **عملکرد بهینه**: اشتراک‌گذاری نتیجه
- ✅ **قابلیت نگهداری**: کد تمیز و قابل نگهداری
- ✅ **مقیاس‌پذیری**: پشتیبانی از بار بالا
- ✅ **قابلیت اطمینان**: مدیریت خطا و timeout 