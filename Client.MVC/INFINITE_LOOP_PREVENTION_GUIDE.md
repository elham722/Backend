# راهنمای جلوگیری از Infinite Loop در Token Refresh

## 🎯 **هدف**

جلوگیری از infinite loop در عملیات refresh token:

1. **Loop Prevention**: جلوگیری از refresh های بی‌نهایت
2. **Session Cleanup**: پاک کردن session در صورت شکست refresh
3. **Event Notification**: اطلاع‌رسانی به سایر کامپوننت‌ها
4. **Graceful Degradation**: مدیریت graceful در صورت خطا

## 🏗️ **معماری Infinite Loop Prevention**

### **1. Failure Detection**
```csharp
// ✅ Detect refresh failures and clear session immediately
if (!refreshSuccess)
{
    ClearSessionAndNotifyLogout();
    return false;
}
```

### **2. Session Cleanup**
```csharp
// ✅ Clear session on any refresh failure
private void ClearSessionAndNotifyLogout()
{
    try
    {
        _logger.LogInformation("Clearing user session due to refresh token failure");
        _userSessionService.ClearUserSession();
        
        // Trigger logout event if available
        OnLogoutRequired?.Invoke(this, EventArgs.Empty);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error clearing user session");
    }
}
```

### **3. Event Notification**
```csharp
// ✅ Event to notify other components about logout requirement
public event EventHandler? OnLogoutRequired;
```

## 🔧 **Implementation Details**

### **1. Refresh Token Method with Loop Prevention**
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
        var refreshToken = _userSessionService.GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("No refresh token available");
            ClearSessionAndNotifyLogout(); // ✅ Prevent loop
            return false;
        }

        var response = await httpClient.PostAsync("api/Auth/refresh-token", content);
        
        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<AuthResultDto>(responseContent, _jsonOptions);
            
            if (result?.IsSuccess == true && !string.IsNullOrEmpty(result.AccessToken))
            {
                _userSessionService.SetUserSession(result);
                return true;
            }
            else
            {
                _logger.LogWarning("Token refresh failed: {Error}", result?.ErrorMessage);
                ClearSessionAndNotifyLogout(); // ✅ Prevent loop
                return false;
            }
        }
        else
        {
            _logger.LogWarning("Token refresh failed with status {StatusCode}", response.StatusCode);
            ClearSessionAndNotifyLogout(); // ✅ Prevent loop
            return false;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error refreshing token");
        ClearSessionAndNotifyLogout(); // ✅ Prevent loop
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

### **2. Authentication Error Handling**
```csharp
public async Task<bool> HandleAuthenticationErrorAsync(HttpResponseMessage response)
{
    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        _logger.LogWarning("Received 401 Unauthorized, attempting token refresh");
        
        var refreshSuccess = await RefreshTokenIfNeededAsync();
        if (refreshSuccess)
        {
            _logger.LogInformation("Token refreshed successfully after 401 error");
            return true;
        }
        else
        {
            _logger.LogWarning("Failed to refresh token after 401 error - session cleared");
            // ✅ Session is already cleared by RefreshTokenIfNeededAsync
            return false;
        }
    }
    
    return false;
}
```

### **3. Event Subscription**
```csharp
public AuthenticatedHttpClient(/* dependencies */)
{
    // ... initialization ...
    
    // Subscribe to logout required event
    _authInterceptor.OnLogoutRequired += OnLogoutRequired;
}

private void OnLogoutRequired(object? sender, EventArgs e)
{
    _logger.LogWarning("Logout required due to refresh token failure");
    
    // Clear any custom headers that might be related to authentication
    _customHeaders.Clear();
    
    // Additional cleanup can be added here
}
```

## 🚀 **Usage Patterns**

### **1. Automatic Session Cleanup**
```csharp
// ✅ Automatic cleanup on any refresh failure
if (!refreshSuccess)
{
    // Session is automatically cleared
    // Event is automatically triggered
    // No infinite loop possible
    return false;
}
```

### **2. Event-Driven Logout**
```csharp
// ✅ Subscribe to logout events
_authInterceptor.OnLogoutRequired += (sender, e) =>
{
    // Handle logout requirement
    // Redirect to login page
    // Clear local state
    // Notify user
};
```

### **3. Graceful Error Handling**
```csharp
// ✅ Graceful handling of refresh failures
try
{
    var refreshSuccess = await RefreshTokenIfNeededAsync();
    if (!refreshSuccess)
    {
        // Session is already cleared
        // Redirect to login
        return RedirectToAction("Login");
    }
}
catch (Exception ex)
{
    // Session is already cleared
    // Handle exception gracefully
}
```

## 📊 **Benefits**

### **1. Loop Prevention**
```csharp
// ❌ Before: Potential infinite loop
// Refresh fails → Try again → Refresh fails → Try again → ...

// ✅ After: Immediate cleanup
// Refresh fails → Clear session → Stop trying → Redirect to login
```

### **2. Resource Protection**
```csharp
// ✅ Prevents resource exhaustion
// No repeated API calls
// No repeated token validation
// Immediate cleanup on failure
```

### **3. User Experience**
```csharp
// ✅ Better user experience
// Immediate feedback on authentication failure
// Automatic redirect to login
// No hanging requests
```

### **4. System Stability**
```csharp
// ✅ System stability
// No infinite loops
// Proper resource cleanup
// Event-driven architecture
```

## 🔧 **Error Handling Strategies**

### **1. Immediate Cleanup**
```csharp
// ✅ Immediate cleanup on any failure
private void ClearSessionAndNotifyLogout()
{
    try
    {
        _logger.LogInformation("Clearing user session due to refresh token failure");
        _userSessionService.ClearUserSession();
        OnLogoutRequired?.Invoke(this, EventArgs.Empty);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error clearing user session");
    }
}
```

### **2. Event-Driven Architecture**
```csharp
// ✅ Event-driven logout notification
public event EventHandler? OnLogoutRequired;

// Components can subscribe to this event
_authInterceptor.OnLogoutRequired += HandleLogoutRequired;
```

### **3. Graceful Degradation**
```csharp
// ✅ Graceful degradation on failure
if (!refreshSuccess)
{
    // Don't throw exceptions
    // Clear session
    // Notify components
    // Return false for graceful handling
    return false;
}
```

## 📈 **Performance Considerations**

### **1. Immediate Failure Detection**
```csharp
// ✅ Immediate failure detection
// No retry loops
// No exponential backoff
// Immediate cleanup
```

### **2. Resource Cleanup**
```csharp
// ✅ Proper resource cleanup
// Clear session immediately
// Clear custom headers
// Notify other components
```

### **3. Event Efficiency**
```csharp
// ✅ Efficient event handling
// Single event per failure
// No repeated notifications
// Immediate propagation
```

## ✅ **Best Practices**

### **1. Always Clear Session on Failure**
```csharp
// ✅ Always clear session on refresh failure
if (!refreshSuccess)
{
    ClearSessionAndNotifyLogout();
    return false;
}
```

### **2. Use Events for Notification**
```csharp
// ✅ Use events for component notification
public event EventHandler? OnLogoutRequired;

// Subscribe in components
_authInterceptor.OnLogoutRequired += OnLogoutRequired;
```

### **3. Graceful Error Handling**
```csharp
// ✅ Graceful error handling
try
{
    // Refresh operation
}
catch (Exception ex)
{
    ClearSessionAndNotifyLogout();
    return false;
}
```

### **4. Comprehensive Logging**
```csharp
// ✅ Comprehensive logging
_logger.LogInformation("Clearing user session due to refresh token failure");
_logger.LogWarning("Failed to refresh token after 401 error - session cleared");
_logger.LogWarning("Logout required due to refresh token failure");
```

## 🔄 **Migration Guide**

### **1. Before (Potential Infinite Loop)**
```csharp
// ❌ Old way - potential infinite loop
public async Task<bool> RefreshTokenIfNeededAsync()
{
    var refreshToken = _userSessionService.GetRefreshToken();
    var response = await httpClient.PostAsync("api/Auth/refresh-token", content);
    
    if (!response.IsSuccessStatusCode)
    {
        // ❌ No cleanup - potential for infinite loop
        return false;
    }
    
    return true;
}
```

### **2. After (Loop Prevention)**
```csharp
// ✅ New way - loop prevention with cleanup
public async Task<bool> RefreshTokenIfNeededAsync()
{
    try
    {
        var refreshToken = _userSessionService.GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken))
        {
            ClearSessionAndNotifyLogout(); // ✅ Immediate cleanup
            return false;
        }

        var response = await httpClient.PostAsync("api/Auth/refresh-token", content);
        
        if (!response.IsSuccessStatusCode)
        {
            ClearSessionAndNotifyLogout(); // ✅ Immediate cleanup
            return false;
        }
        
        return true;
    }
    catch (Exception ex)
    {
        ClearSessionAndNotifyLogout(); // ✅ Immediate cleanup
        return false;
    }
}
```

### **3. Consumer Updates**
```csharp
// ✅ Updated consumer pattern
_authInterceptor.OnLogoutRequired += (sender, e) =>
{
    // Handle logout requirement
    // Redirect to login
    // Clear local state
};

var refreshSuccess = await _authInterceptor.RefreshTokenIfNeededAsync();
if (!refreshSuccess)
{
    // Session is already cleared
    // Handle gracefully
}
```

## 🛡️ **Security Considerations**

### **1. Immediate Session Cleanup**
```csharp
// ✅ Immediate session cleanup
// Prevents token leakage
// Ensures proper logout
// Maintains security state
```

### **2. Event Security**
```csharp
// ✅ Secure event handling
// No sensitive data in events
// Proper event cleanup
// Secure event propagation
```

### **3. Error Information**
```csharp
// ✅ Secure error handling
// No sensitive data in logs
// Proper error sanitization
// Secure error propagation
```

## 🔍 **Debugging and Monitoring**

### **1. Comprehensive Logging**
```csharp
// ✅ Comprehensive logging for debugging
_logger.LogInformation("Clearing user session due to refresh token failure");
_logger.LogWarning("Failed to refresh token after 401 error - session cleared");
_logger.LogWarning("Logout required due to refresh token failure");
```

### **2. Event Monitoring**
```csharp
// ✅ Monitor logout events
_authInterceptor.OnLogoutRequired += (sender, e) =>
{
    _logger.LogInformation("Logout event triggered");
    // Monitor and track logout events
};
```

### **3. Performance Metrics**
```csharp
// ✅ Track refresh failures
if (!refreshSuccess)
{
    _metrics.IncrementCounter("refresh_failure");
    ClearSessionAndNotifyLogout();
}
```

این سیستم تضمین می‌کند که هیچ infinite loop در عملیات refresh token رخ ندهد و session به صورت مناسب پاک شود. 