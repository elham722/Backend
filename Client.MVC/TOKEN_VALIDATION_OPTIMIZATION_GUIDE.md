# راهنمای بهینه‌سازی Token Validation

## 🎯 **هدف**

بهینه‌سازی validation توکن‌ها با استفاده از cached expiration:

1. **Performance**: افزایش سرعت validation با cache کردن expiration
2. **Efficiency**: کاهش عملیات string split و base64 decode
3. **Optimization**: استفاده از cached data به جای decode مجدد
4. **Fallback**: استفاده از manual decoding در صورت عدم وجود cache

## 🏗️ **معماری Token Validation Optimization**

### **1. Cache Storage**
```csharp
// ✅ Token expiration cache for optimization
private DateTimeOffset? _cachedTokenExpiration;
private string? _cachedTokenHash;
```

### **2. Cache Population**
```csharp
// ✅ Cache token expiration when setting session
private void CacheTokenExpiration(string token)
{
    // Parse JWT token to extract expiration
    // Cache expiration and token hash for validation
    _cachedTokenExpiration = expirationTime;
    _cachedTokenHash = GetTokenHash(token);
}
```

### **3. Cache Validation**
```csharp
// ✅ Use cached expiration for optimization
public DateTimeOffset? GetCachedTokenExpiration(string currentToken)
{
    // Validate that the cached expiration is for the current token
    var currentTokenHash = GetTokenHash(currentToken);
    if (currentTokenHash != _cachedTokenHash)
    {
        // Clear cache on mismatch
        return null;
    }
    return _cachedTokenExpiration;
}
```

## 🔧 **Implementation Details**

### **1. Cache Token Expiration**
```csharp
/// <summary>
/// Cache token expiration for optimization
/// </summary>
private void CacheTokenExpiration(string token)
{
    try
    {
        // Parse JWT token to extract expiration
        var tokenParts = token.Split('.');
        if (tokenParts.Length != 3)
        {
            _logger.LogWarning("Invalid JWT token format");
            return;
        }

        // Decode payload (second part)
        var payload = tokenParts[1];
        var paddedPayload = payload.PadRight(4 * ((payload.Length + 3) / 4), '=');
        var decodedPayload = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
        var payloadJson = System.Text.Encoding.UTF8.GetString(decodedPayload);
        
        var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson, _jsonOptions);
        
        // Extract expiration
        if (payloadData.TryGetProperty("exp", out var expElement))
        {
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
            
            // Cache expiration and token hash for validation
            _cachedTokenExpiration = expirationTime;
            _cachedTokenHash = GetTokenHash(token);
            
            _logger.LogDebug("Token expiration cached: {Expiration}", expirationTime);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error caching token expiration");
        // Clear cache on error
        _cachedTokenExpiration = null;
        _cachedTokenHash = null;
    }
}
```

### **2. Token Hash for Cache Validation**
```csharp
/// <summary>
/// Get simple hash of token for cache validation
/// </summary>
private string GetTokenHash(string token)
{
    // Simple hash for cache validation - not for security
    return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
        System.Text.Encoding.UTF8.GetBytes(token)));
}
```

### **3. Get Cached Expiration**
```csharp
/// <summary>
/// Get cached token expiration if available and valid
/// </summary>
public DateTimeOffset? GetCachedTokenExpiration(string currentToken)
{
    try
    {
        if (_cachedTokenExpiration == null || _cachedTokenHash == null)
        {
            return null;
        }

        // Validate that the cached expiration is for the current token
        var currentTokenHash = GetTokenHash(currentToken);
        if (currentTokenHash != _cachedTokenHash)
        {
            _logger.LogDebug("Token hash mismatch, clearing cache");
            _cachedTokenExpiration = null;
            _cachedTokenHash = null;
            return null;
        }

        return _cachedTokenExpiration;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting cached token expiration");
        return null;
    }
}
```

### **4. Optimized Token Validation**
```csharp
/// <summary>
/// Check if token is valid with optimization
/// </summary>
public bool IsTokenValid()
{
    try
    {
        var token = GetCurrentToken();
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        // ✅ Use cached token expiration for optimization
        var cachedExpiration = _userSessionService.GetCachedTokenExpiration(token);
        if (cachedExpiration.HasValue)
        {
            var currentTime = DateTimeOffset.UtcNow;
            
            // Token is valid if it expires in more than 5 minutes
            var isValid = cachedExpiration.Value > currentTime.AddMinutes(5);
            
            if (!isValid)
            {
                _logger.LogDebug("Token is expired or expiring soon (cached). Expires: {Expiration}, Current: {Current}", 
                    cachedExpiration.Value, currentTime);
            }
            
            return isValid;
        }

        // Fallback to manual decoding if cache is not available
        _logger.LogDebug("No cached expiration available, falling back to manual decoding");
        
        // Manual decoding logic...
        return isValid;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error validating token");
        return false;
    }
}
```

## 🚀 **Usage Patterns**

### **1. Cache Population on Token Set**
```csharp
// ✅ Cache expiration when setting user session
private void SetUserSessionInternal(AuthResultDto result)
{
    // Store JWT token in HttpOnly cookie
    response.Cookies.Append("jwt_token", result.AccessToken, jwtCookieOptions);
    
    // ✅ Cache token expiration for optimization
    CacheTokenExpiration(result.AccessToken);
}
```

### **2. Cache Refresh on Token Update**
```csharp
// ✅ Cache new expiration when refreshing token
public void RefreshJwtToken(string newToken, DateTime? expiresAt = null)
{
    // Update cookie
    response.Cookies.Append("jwt_token", newToken, jwtCookieOptions);
    
    // ✅ Cache new token expiration for optimization
    CacheTokenExpiration(newToken);
}
```

### **3. Optimized Validation**
```csharp
// ✅ Use cached expiration for fast validation
public bool IsTokenValid()
{
    var cachedExpiration = _userSessionService.GetCachedTokenExpiration(token);
    if (cachedExpiration.HasValue)
    {
        // Fast validation using cached data
        return cachedExpiration.Value > DateTimeOffset.UtcNow.AddMinutes(5);
    }
    
    // Fallback to manual decoding
    return ManualTokenValidation(token);
}
```

## 📊 **Benefits**

### **1. Performance Improvement**
```csharp
// ✅ Significant performance improvement
// Before: String split + Base64 decode + JSON deserialize (every time)
// After: Simple hash comparison + cached DateTimeOffset (most times)
```

### **2. Resource Efficiency**
```csharp
// ✅ Reduced CPU usage
// No repeated string operations
// No repeated base64 decoding
// No repeated JSON deserialization
```

### **3. Memory Optimization**
```csharp
// ✅ Better memory usage
// Single decode operation per token
// Cached data reused multiple times
// Reduced garbage collection pressure
```

### **4. Scalability**
```csharp
// ✅ Better scalability
// Faster token validation under load
// Reduced server resource consumption
// Better response times
```

## 🔧 **Error Handling Strategies**

### **1. Cache Invalidation**
```csharp
// ✅ Clear cache on errors
catch (Exception ex)
{
    _logger.LogError(ex, "Error caching token expiration");
    // Clear cache on error
    _cachedTokenExpiration = null;
    _cachedTokenHash = null;
}
```

### **2. Hash Mismatch Handling**
```csharp
// ✅ Handle token hash mismatch
if (currentTokenHash != _cachedTokenHash)
{
    _logger.LogDebug("Token hash mismatch, clearing cache");
    _cachedTokenExpiration = null;
    _cachedTokenHash = null;
    return null;
}
```

### **3. Fallback Strategy**
```csharp
// ✅ Fallback to manual decoding
if (cachedExpiration.HasValue)
{
    // Use cached data
    return ValidateWithCachedData(cachedExpiration.Value);
}

// Fallback to manual decoding if cache is not available
_logger.LogDebug("No cached expiration available, falling back to manual decoding");
return ManualTokenValidation(token);
```

## 📈 **Performance Considerations**

### **1. Cache Hit Rate**
```csharp
// ✅ High cache hit rate expected
// Cache populated on token set/refresh
// Cache validated with hash comparison
// Fallback only when cache is invalid
```

### **2. Memory Usage**
```csharp
// ✅ Minimal memory overhead
// Only two additional fields per service instance
// Hash is small (44 bytes base64)
// DateTimeOffset is 8 bytes
```

### **3. CPU Usage**
```csharp
// ✅ Reduced CPU usage
// Hash comparison vs full JWT decode
// Single decode operation per token lifecycle
// Efficient cache validation
```

### **4. Network Impact**
```csharp
// ✅ No network impact
// All optimizations are local
// No additional API calls
// No external dependencies
```

## ✅ **Best Practices**

### **1. Cache Population**
```csharp
// ✅ Always cache on token operations
public void SetUserSession(AuthResultDto result)
{
    // Set token
    response.Cookies.Append("jwt_token", result.AccessToken, jwtCookieOptions);
    
    // ✅ Cache expiration immediately
    CacheTokenExpiration(result.AccessToken);
}
```

### **2. Cache Validation**
```csharp
// ✅ Always validate cache integrity
public DateTimeOffset? GetCachedTokenExpiration(string currentToken)
{
    // Validate hash before using cached data
    var currentTokenHash = GetTokenHash(currentToken);
    if (currentTokenHash != _cachedTokenHash)
    {
        // Clear invalid cache
        return null;
    }
    return _cachedTokenExpiration;
}
```

### **3. Fallback Strategy**
```csharp
// ✅ Always provide fallback
public bool IsTokenValid()
{
    // Try cached validation first
    var cachedExpiration = GetCachedTokenExpiration(token);
    if (cachedExpiration.HasValue)
    {
        return ValidateWithCachedData(cachedExpiration.Value);
    }
    
    // Fallback to manual validation
    return ManualTokenValidation(token);
}
```

### **4. Error Handling**
```csharp
// ✅ Handle all error scenarios
try
{
    // Cache operations
}
catch (Exception ex)
{
    // Clear cache on any error
    _cachedTokenExpiration = null;
    _cachedTokenHash = null;
    _logger.LogError(ex, "Error in cache operation");
}
```

## 🔄 **Migration Guide**

### **1. Before (Manual Decoding Every Time)**
```csharp
// ❌ Old way - manual decoding every time
public bool IsTokenValid()
{
    var token = GetCurrentToken();
    if (string.IsNullOrEmpty(token)) return false;
    
    // Manual decode every time
    var tokenParts = token.Split('.');
    var payload = tokenParts[1];
    var decodedPayload = Convert.FromBase64String(paddedPayload);
    var payloadJson = Encoding.UTF8.GetString(decodedPayload);
    var payloadData = JsonSerializer.Deserialize<JsonElement>(payloadJson);
    
    if (payloadData.TryGetProperty("exp", out var expElement))
    {
        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
        return expirationTime > DateTimeOffset.UtcNow.AddMinutes(5);
    }
    
    return false;
}
```

### **2. After (Optimized with Cache)**
```csharp
// ✅ New way - optimized with cache
public bool IsTokenValid()
{
    var token = GetCurrentToken();
    if (string.IsNullOrEmpty(token)) return false;
    
    // ✅ Try cached validation first
    var cachedExpiration = _userSessionService.GetCachedTokenExpiration(token);
    if (cachedExpiration.HasValue)
    {
        var currentTime = DateTimeOffset.UtcNow;
        return cachedExpiration.Value > currentTime.AddMinutes(5);
    }
    
    // Fallback to manual decoding only when needed
    _logger.LogDebug("No cached expiration available, falling back to manual decoding");
    return ManualTokenValidation(token);
}
```

### **3. Cache Population**
```csharp
// ✅ Add cache population to token operations
public void SetUserSession(AuthResultDto result)
{
    // Existing token storage logic
    response.Cookies.Append("jwt_token", result.AccessToken, jwtCookieOptions);
    
    // ✅ Add cache population
    CacheTokenExpiration(result.AccessToken);
}

public void RefreshJwtToken(string newToken, DateTime? expiresAt = null)
{
    // Existing token refresh logic
    response.Cookies.Append("jwt_token", newToken, jwtCookieOptions);
    
    // ✅ Add cache refresh
    CacheTokenExpiration(newToken);
}
```

## 🛡️ **Security Considerations**

### **1. Cache Security**
```csharp
// ✅ Secure cache implementation
// Hash is used only for cache validation, not security
// No sensitive data stored in cache
// Cache cleared on errors or mismatches
```

### **2. Token Validation**
```csharp
// ✅ Maintained security
// Same validation logic as before
// Cache only optimizes performance
// No security compromises
```

### **3. Error Handling**
```csharp
// ✅ Secure error handling
// Cache cleared on any error
// Fallback to manual validation
// No sensitive data exposure
```

## 🔍 **Debugging and Monitoring**

### **1. Cache Hit Monitoring**
```csharp
// ✅ Monitor cache effectiveness
_logger.LogDebug("Token validation using cached expiration: {Expiration}", cachedExpiration);
_logger.LogDebug("No cached expiration available, falling back to manual decoding");
```

### **2. Performance Metrics**
```csharp
// ✅ Track performance improvements
var stopwatch = Stopwatch.StartNew();
var isValid = IsTokenValid();
stopwatch.Stop();

_logger.LogDebug("Token validation completed in {ElapsedMs}ms (cached: {IsCached})", 
    stopwatch.ElapsedMilliseconds, cachedExpiration.HasValue);
```

### **3. Cache Statistics**
```csharp
// ✅ Track cache statistics
if (cachedExpiration.HasValue)
{
    _cacheHitCount++;
    _logger.LogDebug("Cache hit #{HitCount}", _cacheHitCount);
}
else
{
    _cacheMissCount++;
    _logger.LogDebug("Cache miss #{MissCount}", _cacheMissCount);
}
```

این سیستم تضمین می‌کند که validation توکن‌ها با حداکثر بهینه‌سازی و حداقل overhead انجام شود. 