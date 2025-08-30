# راهنمای Redis Token Caching

## 🎯 **خلاصه پیاده‌سازی**

✅ **Redis Cache برای JWT و Refresh Token پیاده‌سازی شده:**

### 🔧 **ویژگی‌های اصلی:**
- **JWT Token Caching**: ذخیره JWT tokenها در Redis با TTL
- **Refresh Token Caching**: ذخیره refresh tokenها با device info
- **Token Rotation**: automatic token rotation برای امنیت
- **Device Binding**: هر token با device info bind شده
- **Background Cleanup**: automatic cleanup از expired tokens
- **Health Monitoring**: health check برای Redis

## 🏗️ **معماری پیاده‌سازی**

### **1. TokenCacheService:**
```csharp
public interface ITokenCacheService
{
    Task<bool> StoreJwtTokenAsync(string userId, string tokenId, string token, DateTime expiresAt);
    Task<bool> StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt, string? deviceInfo = null, string? ipAddress = null);
    Task<string?> GetJwtTokenAsync(string userId, string tokenId);
    Task<string?> GetRefreshTokenAsync(string userId, string refreshToken);
    Task<bool> InvalidateJwtTokenAsync(string userId, string tokenId);
    Task<bool> InvalidateRefreshTokenAsync(string userId, string refreshToken);
    Task<bool> InvalidateAllUserTokensAsync(string userId);
    Task<bool> IsTokenValidAsync(string userId, string tokenId);
    Task<bool> IsRefreshTokenValidAsync(string userId, string refreshToken);
    Task<int> GetActiveTokenCountAsync(string userId);
    Task<bool> RotateTokensIfNeededAsync(string userId, string currentTokenId, DateTime currentExpiry);
    Task<Dictionary<string, object>> GetTokenInfoAsync(string userId, string tokenId);
    Task CleanupExpiredTokensAsync();
}
```

### **2. Redis Key Structure:**
```
jwt:{userId}:{tokenId} -> JWT Token Info
refresh:{userId}:{refreshToken} -> Refresh Token Info
user_tokens:{userId} -> User's Active Tokens Set
```

### **3. Token Info Structure:**
```json
{
  "Token": "jwt_token_string",
  "ExpiresAt": "2024-01-01T12:00:00Z",
  "CreatedAt": "2024-01-01T11:45:00Z",
  "IsActive": true,
  "Type": "JWT|Refresh",
  "DeviceInfo": "device_info",
  "IpAddress": "ip_address"
}
```

## 🔧 **تنظیمات**

### **1. TokenCache Configuration:**
```json
{
  "TokenCache": {
    "JwtTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7,
    "MaxTokensPerUser": 5,
    "EnableTokenRotation": true,
    "TokenRotationThresholdMinutes": 5
  }
}
```

### **2. Redis Configuration:**
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "Backend:",
    "DefaultDatabase": 0,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "ConnectRetry": 3,
    "ReconnectRetryPolicy": "LinearRetry",
    "KeepAlive": 180
  }
}
```

### **3. TokenCleanup Configuration:**
```json
{
  "TokenCleanup": {
    "CleanupInterval": "00:15:00",
    "EnableCleanup": true,
    "MaxTokensPerCleanup": 1000
  }
}
```

## 🚀 **نحوه استفاده**

### **1. در AccountManagementService:**
```csharp
public async Task<string> GenerateAccessTokenAsync(List<Claim> claims, string userId)
{
    // Generate JWT token
    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    
    // Generate unique token ID for caching
    var tokenId = Guid.NewGuid().ToString();
    
    // Store token in Redis cache
    var cacheSuccess = await _tokenCache.StoreJwtTokenAsync(userId, tokenId, tokenString, expiresAt);
    
    return tokenString;
}

public async Task<string> GenerateRefreshTokenAsync(string userId, string? deviceInfo = null, string? ipAddress = null)
{
    var refreshToken = Guid.NewGuid().ToString() + "_" + DateTime.UtcNow.Ticks;
    var expiresAt = DateTime.UtcNow.AddDays(7);
    
    // Store refresh token in Redis cache
    await _tokenCache.StoreRefreshTokenAsync(userId, refreshToken, expiresAt, deviceInfo, ipAddress);
    
    return refreshToken;
}
```

### **2. در UserService:**
```csharp
// Generate JWT tokens with caching
var claims = await _accountManagementService.GetUserClaimsAsync(user);
var accessToken = await _accountManagementService.GenerateAccessTokenAsync(claims, user.Id);
var refreshToken = await _accountManagementService.GenerateRefreshTokenAsync(user.Id, loginDto.DeviceInfo, loginDto.IpAddress);
```

### **3. Token Validation:**
```csharp
// Check if JWT token is valid
var isValid = await _tokenCache.IsTokenValidAsync(userId, tokenId);

// Check if refresh token is valid
var isRefreshValid = await _tokenCache.IsRefreshTokenValidAsync(userId, refreshToken);
```

### **4. Token Invalidation:**
```csharp
// Invalidate specific JWT token
await _tokenCache.InvalidateJwtTokenAsync(userId, tokenId);

// Invalidate specific refresh token
await _tokenCache.InvalidateRefreshTokenAsync(userId, refreshToken);

// Invalidate all user tokens (logout from all devices)
await _tokenCache.InvalidateAllUserTokensAsync(userId);
```

## 🔒 **ویژگی‌های امنیتی**

### **1. Token Rotation:**
- **Automatic**: توکن‌ها قبل از expire شدن rotate می‌شوند
- **Configurable**: قابل تنظیم از طریق configuration
- **Threshold**: تنظیم threshold برای rotation

### **2. Device Binding:**
- **Device Info**: هر token با device info bind شده
- **IP Address**: tracking IP address برای security
- **User Agent**: tracking user agent برای validation

### **3. Token Management:**
- **Unique IDs**: هر token unique ID داره
- **Expiration**: automatic expiration با Redis TTL
- **Revocation**: امکان revoke کردن tokenها

## 📊 **Performance & Scalability**

### **1. Redis Benefits:**
- **Fast Access**: دسترسی سریع به tokenها
- **Distributed**: پشتیبانی از multiple instances
- **TTL**: automatic expiration
- **Atomic Operations**: عملیات atomic

### **2. Caching Strategy:**
- **Lazy Loading**: tokenها فقط در صورت نیاز load می‌شوند
- **TTL-based Cleanup**: automatic cleanup با Redis TTL
- **Background Service**: cleanup در background

### **3. Monitoring:**
- **Health Checks**: monitoring سلامت Redis
- **Logging**: کامل logging از operations
- **Metrics**: tracking performance metrics

## 🧪 **Testing**

### **1. Unit Tests:**
```csharp
[Test]
public async Task StoreJwtTokenAsync_ShouldStoreTokenInCache()
{
    // Arrange
    var userId = "user123";
    var tokenId = "token456";
    var token = "jwt_token_string";
    var expiresAt = DateTime.UtcNow.AddMinutes(15);

    // Act
    var result = await _tokenCacheService.StoreJwtTokenAsync(userId, tokenId, token, expiresAt);

    // Assert
    Assert.IsTrue(result);
    
    var cachedToken = await _tokenCacheService.GetJwtTokenAsync(userId, tokenId);
    Assert.AreEqual(token, cachedToken);
}
```

### **2. Integration Tests:**
```csharp
[Test]
public async Task TokenLifecycle_ShouldWorkEndToEnd()
{
    // Store token
    await _tokenCacheService.StoreJwtTokenAsync(userId, tokenId, token, expiresAt);
    
    // Validate token
    var isValid = await _tokenCacheService.IsTokenValidAsync(userId, tokenId);
    Assert.IsTrue(isValid);
    
    // Invalidate token
    await _tokenCacheService.InvalidateJwtTokenAsync(userId, tokenId);
    
    // Check invalidation
    var cachedToken = await _tokenCacheService.GetJwtTokenAsync(userId, tokenId);
    Assert.IsNull(cachedToken);
}
```

## 🚀 **Deployment**

### **1. Development:**
```bash
# Start Redis locally
docker run -d -p 6379:6379 redis:alpine

# Update connection string
"Redis": {
  "ConnectionString": "localhost:6379"
}
```

### **2. Production:**
```bash
# Redis Cluster
"Redis": {
  "ConnectionString": "redis-cluster.example.com:6379,password=your-password",
  "InstanceName": "Backend:Prod:",
  "ConnectTimeout": 10000,
  "SyncTimeout": 10000
}
```

### **3. Health Monitoring:**
```bash
# Health check endpoint
GET /health

# Redis-specific health check
GET /health/redis_cache
```

## 📋 **Best Practices**

### **1. Security:**
- ✅ **Token Rotation**: enable automatic token rotation
- ✅ **Device Binding**: bind tokens to device info
- ✅ **IP Tracking**: track IP addresses for security
- ✅ **Expiration**: set appropriate expiration times

### **2. Performance:**
- ✅ **TTL-based Cleanup**: rely on Redis TTL for expiration
- ✅ **Background Service**: use background service for cleanup
- ✅ **Lazy Loading**: load tokens only when needed
- ✅ **Connection Pooling**: configure Redis connection pooling

### **3. Monitoring:**
- ✅ **Health Checks**: implement Redis health checks
- ✅ **Logging**: comprehensive logging for debugging
- ✅ **Metrics**: track cache hit/miss ratios
- ✅ **Alerts**: set up alerts for Redis failures

## 🔍 **Troubleshooting**

### **1. Common Issues:**
- **Connection Failures**: check Redis connection string
- **Memory Issues**: monitor Redis memory usage
- **Performance Issues**: check Redis performance metrics
- **Token Expiration**: verify TTL settings

### **2. Debug Commands:**
```bash
# Redis CLI
redis-cli -h localhost -p 6379

# Check keys
KEYS *

# Check TTL
TTL jwt:user123:token456

# Monitor operations
MONITOR
```

### **3. Log Analysis:**
```csharp
// Enable debug logging
"Logging": {
  "LogLevel": {
    "Backend.Infrastructure.Cache": "Debug"
  }
}
```

## 📚 **References**

- [Redis Documentation](https://redis.io/documentation)
- [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Background Services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) 