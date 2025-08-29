# راهنمای کامل Thread Safety در Token Refresh

## 🎯 **هدف**

جلوگیری از race condition در عملیات refresh token:

1. **Race Condition Prevention**: جلوگیری از چندین refresh همزمان
2. **Resource Protection**: محافظت از refresh token در برابر مصرف چندباره
3. **Performance Optimization**: کاهش درخواست‌های اضافی به سرور
4. **Reliability**: تضمین عملکرد صحیح در محیط‌های concurrent

## 🏗️ **معماری Thread Safety**

### **1. Synchronization Mechanisms**
```csharp
// Thread-safety for token refresh operations
private static readonly SemaphoreSlim _refreshLock = new(1, 1);
private static readonly object _refreshStateLock = new();
private static bool _isRefreshing = false;
private static DateTime _lastRefreshAttempt = DateTime.MinValue;
private static readonly TimeSpan _refreshCooldown = TimeSpan.FromSeconds(5);
```

### **2. Lock Strategy**
```csharp
// ✅ Two-level locking strategy
// 1. State Lock: برای بررسی وضعیت refresh
// 2. Semaphore: برای اجرای عملیات refresh
```

## 🔧 **Implementation Details**

### **1. Refresh Token Method**
```csharp
public async Task<bool> RefreshTokenIfNeededAsync()
{
    // Check if we're already refreshing or recently attempted
    lock (_refreshStateLock)
    {
        if (_isRefreshing)
        {
            _logger.LogDebug("Token refresh already in progress, waiting for completion");
            return false; // Let the caller handle this case
        }
        
        if (DateTime.UtcNow - _lastRefreshAttempt < _refreshCooldown)
        {
            _logger.LogDebug("Token refresh attempted too recently, skipping");
            return false;
        }
        
        _isRefreshing = true;
        _lastRefreshAttempt = DateTime.UtcNow;
    }

    await _refreshLock.WaitAsync();
    try
    {
        // Perform actual refresh operation
        return await PerformTokenRefreshAsync();
    }
    finally
    {
        _refreshLock.Release();
        
        // Reset refresh state
        lock (_refreshStateLock)
        {
            _isRefreshing = false;
        }
    }
}
```

### **2. Wait for Completion Method**
```csharp
public async Task<bool> WaitForRefreshCompletionAsync(TimeSpan timeout = default)
{
    if (timeout == default)
    {
        timeout = TimeSpan.FromSeconds(30); // Default timeout
    }

    var startTime = DateTime.UtcNow;
    
    while (DateTime.UtcNow - startTime < timeout)
    {
        lock (_refreshStateLock)
        {
            if (!_isRefreshing)
            {
                return true; // Refresh completed or not in progress
            }
        }
        
        await Task.Delay(100); // Wait 100ms before checking again
    }
    
    _logger.LogWarning("Timeout waiting for token refresh completion");
    return false;
}
```

### **3. HTTP Client Integration**
```csharp
// Handle authentication errors with thread-safety
if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
    httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
{
    var handled = await _authInterceptor.HandleAuthenticationErrorAsync(httpResponse);
    if (handled)
    {
        // Wait for refresh completion before retrying
        var refreshCompleted = await _authInterceptor.WaitForRefreshCompletionAsync();
        if (refreshCompleted)
        {
            // Retry the request with new token
            request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request = await _authInterceptor.AddAuthenticationHeaderAsync(request);
            AddCustomHeadersToRequest(request);
            httpResponse = await httpClient.SendAsync(request, cancellationToken);
        }
    }
}
```

## 🚀 **Usage Patterns**

### **1. Concurrent Request Handling**
```csharp
// ✅ Scenario: Multiple requests with expired token
// Request 1: Starts refresh
// Request 2: Waits for refresh completion
// Request 3: Waits for refresh completion
// All requests use the same refreshed token
```

### **2. Cooldown Protection**
```csharp
// ✅ Prevents excessive refresh attempts
if (DateTime.UtcNow - _lastRefreshAttempt < _refreshCooldown)
{
    _logger.LogDebug("Token refresh attempted too recently, skipping");
    return false;
}
```

### **3. Timeout Handling**
```csharp
// ✅ Graceful timeout handling
var refreshCompleted = await _authInterceptor.WaitForRefreshCompletionAsync(TimeSpan.FromSeconds(10));
if (!refreshCompleted)
{
    _logger.LogWarning("Token refresh timeout, proceeding with current token");
    // Handle timeout scenario
}
```

## 📊 **Benefits**

### **1. Race Condition Prevention**
```csharp
// ❌ Before: Multiple concurrent refreshes
// Request 1: Refresh token
// Request 2: Refresh token (duplicate)
// Request 3: Refresh token (duplicate)

// ✅ After: Single refresh with waiting
// Request 1: Refresh token
// Request 2: Wait for completion
// Request 3: Wait for completion
```

### **2. Resource Protection**
```csharp
// ✅ Prevents multiple refresh token consumption
// Only one refresh operation at a time
// Other requests wait for completion
// All requests benefit from the same refresh
```

### **3. Performance Optimization**
```csharp
// ✅ Reduces server load
// Single refresh request instead of multiple
// Cooldown prevents excessive attempts
// Efficient resource utilization
```

### **4. Reliability**
```csharp
// ✅ Consistent behavior in concurrent scenarios
// Predictable token refresh patterns
// Proper error handling
// Timeout protection
```

## 🔧 **Error Handling Strategies**

### **1. Refresh Failure Handling**
```csharp
try
{
    return await PerformTokenRefreshAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error refreshing token");
    return false;
}
finally
{
    _refreshLock.Release();
    
    // Always reset state, even on failure
    lock (_refreshStateLock)
    {
        _isRefreshing = false;
    }
}
```

### **2. Timeout Handling**
```csharp
public async Task<bool> WaitForRefreshCompletionAsync(TimeSpan timeout = default)
{
    if (timeout == default)
    {
        timeout = TimeSpan.FromSeconds(30);
    }

    var startTime = DateTime.UtcNow;
    
    while (DateTime.UtcNow - startTime < timeout)
    {
        lock (_refreshStateLock)
        {
            if (!_isRefreshing)
            {
                return true;
            }
        }
        
        await Task.Delay(100);
    }
    
    _logger.LogWarning("Timeout waiting for token refresh completion");
    return false;
}
```

### **3. Cooldown Protection**
```csharp
// ✅ Prevents rapid refresh attempts
private static readonly TimeSpan _refreshCooldown = TimeSpan.FromSeconds(5);

if (DateTime.UtcNow - _lastRefreshAttempt < _refreshCooldown)
{
    _logger.LogDebug("Token refresh attempted too recently, skipping");
    return false;
}
```

## 📈 **Performance Considerations**

### **1. Lock Granularity**
```csharp
// ✅ Fine-grained locking
// State lock: Only for checking/updating state
// Semaphore: Only for actual refresh operation
// Minimal contention between threads
```

### **2. Polling Strategy**
```csharp
// ✅ Efficient polling
await Task.Delay(100); // 100ms intervals
// Balance between responsiveness and CPU usage
```

### **3. Memory Allocation**
```csharp
// ✅ Static fields for shared state
private static readonly SemaphoreSlim _refreshLock = new(1, 1);
private static readonly object _refreshStateLock = new();
// No per-instance overhead
```

## ✅ **Best Practices**

### **1. Always Use Finally Block**
```csharp
// ✅ Ensure cleanup
await _refreshLock.WaitAsync();
try
{
    // Refresh operation
}
finally
{
    _refreshLock.Release();
    // Reset state
}
```

### **2. Proper State Management**
```csharp
// ✅ Consistent state updates
lock (_refreshStateLock)
{
    _isRefreshing = true;
    _lastRefreshAttempt = DateTime.UtcNow;
}
```

### **3. Comprehensive Logging**
```csharp
// ✅ Detailed logging for debugging
_logger.LogDebug("Token refresh already in progress, waiting for completion");
_logger.LogDebug("Token refresh attempted too recently, skipping");
_logger.LogWarning("Timeout waiting for token refresh completion");
```

### **4. Configurable Timeouts**
```csharp
// ✅ Flexible timeout configuration
public async Task<bool> WaitForRefreshCompletionAsync(TimeSpan timeout = default)
{
    if (timeout == default)
    {
        timeout = TimeSpan.FromSeconds(30); // Configurable default
    }
    // Implementation
}
```

## 🔄 **Migration Guide**

### **1. Before (Race Condition)**
```csharp
// ❌ Old way - potential race conditions
public async Task<bool> RefreshTokenIfNeededAsync()
{
    // No synchronization
    var refreshToken = _userSessionService.GetRefreshToken();
    var response = await httpClient.PostAsync("api/Auth/refresh-token", content);
    // Multiple requests could execute this simultaneously
}
```

### **2. After (Thread-Safe)**
```csharp
// ✅ New way - thread-safe with synchronization
public async Task<bool> RefreshTokenIfNeededAsync()
{
    // Check state with lock
    lock (_refreshStateLock)
    {
        if (_isRefreshing) return false;
        _isRefreshing = true;
    }

    // Execute with semaphore
    await _refreshLock.WaitAsync();
    try
    {
        // Single refresh operation
        return await PerformTokenRefreshAsync();
    }
    finally
    {
        _refreshLock.Release();
        // Reset state
    }
}
```

### **3. Consumer Updates**
```csharp
// ✅ Updated consumer pattern
if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    var handled = await _authInterceptor.HandleAuthenticationErrorAsync(httpResponse);
    if (handled)
    {
        // Wait for refresh completion
        var refreshCompleted = await _authInterceptor.WaitForRefreshCompletionAsync();
        if (refreshCompleted)
        {
            // Retry with new token
        }
    }
}
```

## 🛡️ **Security Considerations**

### **1. Token Protection**
```csharp
// ✅ Prevents token exhaustion
// Single refresh operation prevents multiple token consumption
// Cooldown prevents rapid refresh attempts
```

### **2. State Consistency**
```csharp
// ✅ Consistent state across threads
// Proper state reset in finally blocks
// Atomic state updates with locks
```

### **3. Error Isolation**
```csharp
// ✅ Isolated error handling
// Errors in one refresh don't affect others
// Proper cleanup on exceptions
```

این سیستم تضمین می‌کند که عملیات refresh token در محیط‌های concurrent به صورت thread-safe و قابل اعتماد اجرا شود. 