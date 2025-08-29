# راهنمای Resilience Policy در Refresh Token

## 🎯 **هدف**

استفاده از resilience policies در عملیات refresh token:

1. **Reliability**: افزایش قابلیت اطمینان در refresh token
2. **Retry Logic**: منطق retry برای عملیات refresh
3. **Circuit Breaker**: محافظت از سیستم در برابر failures
4. **Timeout Protection**: محافظت از timeout در refresh operations

## 🏗️ **معماری Resilience Policy در Refresh**

### **1. Policy Selection**
```csharp
// ✅ Use auth policy for refresh token endpoint
var policy = _resiliencePolicyService.CreateAuthPolicy(); // Use auth policy for refresh
```

### **2. Policy Execution**
```csharp
// ✅ Execute refresh with resilience policy
var response = await policy.ExecuteAsync(async (context) =>
{
    var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/Auth/refresh-token")
    {
        Content = content
    };
    
    _logger.LogDebug("Sending refresh token request with resilience policy");
    return await httpClient.SendAsync(httpRequest);
}, new Polly.Context("refresh-token"));
```

## 🔧 **Implementation Details**

### **1. Constructor Injection**
```csharp
public AuthenticationInterceptor(
    IUserSessionService userSessionService,
    IHttpClientFactory httpClientFactory,
    ILogger<AuthenticationInterceptor> logger,
    ResiliencePolicyService resiliencePolicyService) // ✅ Inject resilience service
{
    _userSessionService = userSessionService ?? throw new ArgumentNullException(nameof(userSessionService));
    _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _resiliencePolicyService = resiliencePolicyService ?? throw new ArgumentNullException(nameof(resiliencePolicyService));
    
    _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
```

### **2. Refresh Token with Resilience**
```csharp
public async Task<bool> RefreshTokenIfNeededAsync()
{
    // Check state and acquire lock
    lock (_refreshStateLock)
    {
        if (_isRefreshing) return false;
        if (DateTime.UtcNow - _lastRefreshAttempt < _refreshCooldown) return false;
        _isRefreshing = true;
        _lastRefreshAttempt = DateTime.UtcNow;
    }

    await _refreshLock.WaitAsync();
    try
    {
        _logger.LogInformation("Starting token refresh operation");
        
        var refreshToken = _userSessionService.GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("No refresh token available");
            ClearSessionAndNotifyLogout();
            return false;
        }

        _logger.LogInformation("Attempting to refresh access token");
        
        // Create refresh token request
        var request = new RefreshTokenDto { RefreshToken = refreshToken };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // ✅ Use resilience policy for refresh token endpoint
        var httpClient = _httpClientFactory.CreateClient("ApiClient");
        var policy = _resiliencePolicyService.CreateAuthPolicy(); // Use auth policy for refresh
        
        var response = await policy.ExecuteAsync(async (context) =>
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/Auth/refresh-token")
            {
                Content = content
            };
            
            _logger.LogDebug("Sending refresh token request with resilience policy");
            return await httpClient.SendAsync(httpRequest);
        }, new Polly.Context("refresh-token"));
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResultDto>(responseContent, _jsonOptions);
            
            if (result?.IsSuccess == true && !string.IsNullOrEmpty(result.AccessToken))
            {
                _userSessionService.SetUserSession(result);
                _logger.LogInformation("Token refreshed successfully");
                return true;
            }
            else
            {
                _logger.LogWarning("Token refresh failed: {Error}", result?.ErrorMessage);
                ClearSessionAndNotifyLogout();
                return false;
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Token refresh failed with status {StatusCode}: {Error}", 
                response.StatusCode, errorContent);
            
            ClearSessionAndNotifyLogout();
            return false;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error refreshing token");
        ClearSessionAndNotifyLogout();
        return false;
    }
    finally
    {
        _refreshLock.Release();
        lock (_refreshStateLock)
        {
            _isRefreshing = false;
        }
    }
}
```

## 🚀 **Usage Patterns**

### **1. Auth Policy for Refresh**
```csharp
// ✅ Use auth policy for sensitive operations
var policy = _resiliencePolicyService.CreateAuthPolicy(); // Retry + Circuit Breaker + Timeout
```

### **2. Context for Monitoring**
```csharp
// ✅ Use context for monitoring and debugging
var response = await policy.ExecuteAsync(async (context) =>
{
    // Refresh operation
}, new Polly.Context("refresh-token"));
```

### **3. Comprehensive Error Handling**
```csharp
// ✅ Handle both policy failures and business logic failures
try
{
    var response = await policy.ExecuteAsync(async (context) =>
    {
        // Refresh operation
    }, new Polly.Context("refresh-token"));
    
    if (response.IsSuccessStatusCode)
    {
        // Handle success
    }
    else
    {
        // Handle HTTP error
        ClearSessionAndNotifyLogout();
    }
}
catch (Exception ex)
{
    // Handle policy failure (timeout, circuit breaker, etc.)
    _logger.LogError(ex, "Error refreshing token");
    ClearSessionAndNotifyLogout();
}
```

## 📊 **Benefits**

### **1. Reliability**
```csharp
// ✅ Increased reliability
// Automatic retry on transient failures
// Circuit breaker prevents cascading failures
// Timeout protection prevents hanging requests
```

### **2. Performance**
```csharp
// ✅ Better performance
// Fast failure detection
// Efficient retry strategies
// Resource protection
```

### **3. Monitoring**
```csharp
// ✅ Better monitoring
// Context-based logging
// Policy execution tracking
// Failure pattern analysis
```

### **4. User Experience**
```csharp
// ✅ Better user experience
// Automatic recovery from transient failures
// Consistent behavior under load
// Reduced error rates
```

## 🔧 **Error Handling Strategies**

### **1. Policy-Level Errors**
```csharp
// ✅ Handle policy execution errors
try
{
    var response = await policy.ExecuteAsync(async (context) =>
    {
        // Refresh operation
    }, new Polly.Context("refresh-token"));
}
catch (TimeoutRejectedException)
{
    _logger.LogWarning("Refresh token request timed out");
    ClearSessionAndNotifyLogout();
}
catch (BrokenCircuitException)
{
    _logger.LogWarning("Circuit breaker is open for refresh token");
    ClearSessionAndNotifyLogout();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error during refresh token");
    ClearSessionAndNotifyLogout();
}
```

### **2. HTTP-Level Errors**
```csharp
// ✅ Handle HTTP response errors
if (response.IsSuccessStatusCode)
{
    // Handle success
}
else
{
    var errorContent = await response.Content.ReadAsStringAsync();
    _logger.LogWarning("Token refresh failed with status {StatusCode}: {Error}", 
        response.StatusCode, errorContent);
    
    ClearSessionAndNotifyLogout();
}
```

### **3. Business Logic Errors**
```csharp
// ✅ Handle business logic errors
var result = JsonSerializer.Deserialize<AuthResultDto>(responseContent, _jsonOptions);

if (result?.IsSuccess == true && !string.IsNullOrEmpty(result.AccessToken))
{
    // Handle success
}
else
{
    _logger.LogWarning("Token refresh failed: {Error}", result?.ErrorMessage);
    ClearSessionAndNotifyLogout();
}
```

## 📈 **Performance Considerations**

### **1. Policy Configuration**
```csharp
// ✅ Optimized policy configuration for refresh operations
// Auth policy includes:
// - Retry with exponential backoff
// - Circuit breaker for repeated failures
// - Timeout protection
// - Appropriate retry conditions
```

### **2. Context Usage**
```csharp
// ✅ Efficient context usage
var context = new Polly.Context("refresh-token");
context["operation"] = "token-refresh";
context["timestamp"] = DateTime.UtcNow;

var response = await policy.ExecuteAsync(async (ctx) =>
{
    _logger.LogDebug("Executing refresh token with context: {Context}", ctx.OperationKey);
    // Refresh operation
}, context);
```

### **3. Resource Management**
```csharp
// ✅ Proper resource management
using var httpClient = _httpClientFactory.CreateClient("ApiClient");
using var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await policy.ExecuteAsync(async (context) =>
{
    using var request = new HttpRequestMessage(HttpMethod.Post, "api/Auth/refresh-token")
    {
        Content = content
    };
    return await httpClient.SendAsync(request);
}, new Polly.Context("refresh-token"));
```

## ✅ **Best Practices**

### **1. Use Appropriate Policy**
```csharp
// ✅ Use auth policy for refresh operations
var policy = _resiliencePolicyService.CreateAuthPolicy(); // Designed for auth operations
```

### **2. Comprehensive Logging**
```csharp
// ✅ Comprehensive logging for debugging
_logger.LogDebug("Sending refresh token request with resilience policy");
_logger.LogInformation("Token refresh operation completed");
_logger.LogWarning("Token refresh failed with policy: {PolicyError}");
```

### **3. Proper Error Handling**
```csharp
// ✅ Handle all types of errors
try
{
    // Policy execution
}
catch (TimeoutRejectedException)
{
    // Handle timeout
}
catch (BrokenCircuitException)
{
    // Handle circuit breaker
}
catch (Exception ex)
{
    // Handle other errors
}
```

### **4. Context for Monitoring**
```csharp
// ✅ Use context for monitoring
var context = new Polly.Context("refresh-token");
context["user_id"] = userId; // If available
context["timestamp"] = DateTime.UtcNow;

var response = await policy.ExecuteAsync(async (ctx) =>
{
    // Refresh operation with context
}, context);
```

## 🔄 **Migration Guide**

### **1. Before (Direct HttpClient)**
```csharp
// ❌ Old way - direct HttpClient usage
var httpClient = _httpClientFactory.CreateClient("ApiClient");
var response = await httpClient.PostAsync("api/Auth/refresh-token", content);

if (response.IsSuccessStatusCode)
{
    // Handle success
}
else
{
    // Handle error
}
```

### **2. After (Resilience Policy)**
```csharp
// ✅ New way - with resilience policy
var httpClient = _httpClientFactory.CreateClient("ApiClient");
var policy = _resiliencePolicyService.CreateAuthPolicy();

var response = await policy.ExecuteAsync(async (context) =>
{
    var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/Auth/refresh-token")
    {
        Content = content
    };
    
    _logger.LogDebug("Sending refresh token request with resilience policy");
    return await httpClient.SendAsync(httpRequest);
}, new Polly.Context("refresh-token"));

if (response.IsSuccessStatusCode)
{
    // Handle success
}
else
{
    // Handle error
}
```

### **3. Constructor Updates**
```csharp
// ✅ Updated constructor with resilience service
public AuthenticationInterceptor(
    IUserSessionService userSessionService,
    IHttpClientFactory httpClientFactory,
    ILogger<AuthenticationInterceptor> logger,
    ResiliencePolicyService resiliencePolicyService) // ✅ Added dependency
{
    _resiliencePolicyService = resiliencePolicyService ?? throw new ArgumentNullException(nameof(resiliencePolicyService));
    // ... other dependencies
}
```

## 🛡️ **Security Considerations**

### **1. Policy Security**
```csharp
// ✅ Secure policy configuration
// Auth policy includes appropriate retry conditions
// Prevents retry on authentication failures
// Proper timeout values
```

### **2. Error Information**
```csharp
// ✅ Secure error handling
// No sensitive data in logs
// Proper error sanitization
// Secure error propagation
```

### **3. Resource Protection**
```csharp
// ✅ Resource protection
// Circuit breaker prevents resource exhaustion
// Timeout prevents hanging connections
// Proper cleanup on failures
```

## 🔍 **Debugging and Monitoring**

### **1. Policy Monitoring**
```csharp
// ✅ Monitor policy execution
var context = new Polly.Context("refresh-token");
context["operation"] = "token-refresh";

var response = await policy.ExecuteAsync(async (ctx) =>
{
    _logger.LogDebug("Executing refresh token with context: {Context}", ctx.OperationKey);
    // Refresh operation
}, context);
```

### **2. Performance Metrics**
```csharp
// ✅ Track performance metrics
var stopwatch = Stopwatch.StartNew();
var response = await policy.ExecuteAsync(async (context) =>
{
    // Refresh operation
}, new Polly.Context("refresh-token"));
stopwatch.Stop();

_logger.LogInformation("Refresh token operation completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
```

### **3. Error Tracking**
```csharp
// ✅ Track errors for analysis
try
{
    var response = await policy.ExecuteAsync(async (context) =>
    {
        // Refresh operation
    }, new Polly.Context("refresh-token"));
}
catch (Exception ex)
{
    _logger.LogError(ex, "Refresh token failed with policy error");
    // Track error for analysis
}
```

این سیستم تضمین می‌کند که عملیات refresh token با قابلیت اطمینان بالا و resilience policies مناسب اجرا شود. 