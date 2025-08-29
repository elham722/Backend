# راهنمای کامل Polly Resilience برای Authentication

## 🎯 **هدف**

این سیستم برای حل مشکلات زیر طراحی شده:

1. **عدم وجود Retry Policy** - درخواست‌های ناموفق retry نمی‌شدند
2. **عدم وجود Circuit Breaker** - درخواست‌های مکرر به سرور down شده
3. **عدم وجود Timeout Management** - درخواست‌ها بی‌نهایت منتظر می‌ماندند
4. **عدم وجود Exponential Backoff** - retry ها با فاصله ثابت انجام می‌شدند

## 🏗️ **معماری Resilience**

### **1. ResiliencePolicyService**
```csharp
// مسئولیت: ایجاد و مدیریت Polly policies
- Retry Policy با Exponential Backoff
- Circuit Breaker Policy
- Timeout Policy
- ترکیب policies برای عملیات مختلف
```

### **2. Policy Types**

#### **Auth Policy (Most Resilient)**
- **Retry**: 3 بار با exponential backoff
- **Circuit Breaker**: 3 خطا = 30 ثانیه break
- **Timeout**: 30 ثانیه
- **Use Case**: Login, Register, Refresh Token

#### **General Policy (Balanced)**
- **Retry**: 2 بار با exponential backoff
- **Circuit Breaker**: 5 خطا = 60 ثانیه break
- **Timeout**: 30 ثانیه
- **Use Case**: PUT, DELETE, PATCH operations

#### **Read-Only Policy (Lightweight)**
- **Retry**: 2 بار با exponential backoff
- **Timeout**: 30 ثانیه
- **Use Case**: GET operations

## 🚀 **نحوه کارکرد**

### **1. Automatic Policy Selection**
```csharp
// در AuthenticatedHttpClient
var isAuthOperation = endpoint.Contains("/auth/", StringComparison.OrdinalIgnoreCase) ||
                    endpoint.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
                    endpoint.Contains("/register", StringComparison.OrdinalIgnoreCase) ||
                    endpoint.Contains("/refresh", StringComparison.OrdinalIgnoreCase);

var policy = isAuthOperation 
    ? _resiliencePolicyService.CreateAuthPolicy() 
    : _resiliencePolicyService.CreateGeneralPolicy();
```

### **2. Policy Execution**
```csharp
var response = await policy.ExecuteAsync(async (context) =>
{
    // HTTP request logic here
    var httpResponse = await httpClient.SendAsync(request, cancellationToken);
    return httpResponse;
}, new Context(endpoint));
```

## ⚙️ **تنظیمات Configuration**

### **appsettings.json**
```json
{
  "ResiliencePolicies": {
    "Auth": {
      "RetryCount": 3,
      "RetryDelaySeconds": 2,
      "CircuitBreakerThreshold": 3,
      "CircuitBreakerDurationSeconds": 30,
      "TimeoutSeconds": 30
    },
    "General": {
      "RetryCount": 2,
      "RetryDelaySeconds": 1,
      "CircuitBreakerThreshold": 5,
      "CircuitBreakerDurationSeconds": 60,
      "TimeoutSeconds": 30
    },
    "ReadOnly": {
      "RetryCount": 2,
      "RetryDelaySeconds": 1,
      "TimeoutSeconds": 30
    }
  }
}
```

### **appsettings.Development.json**
```json
{
  "ResiliencePolicies": {
    "Auth": {
      "RetryCount": 2,
      "RetryDelaySeconds": 1,
      "CircuitBreakerThreshold": 2,
      "CircuitBreakerDurationSeconds": 15,
      "TimeoutSeconds": 15
    }
  }
}
```

## 🔧 **Error Handling**

### **1. Transient Errors (Retry)**
- `HttpRequestException`
- `SocketException`
- `HttpStatusCode.TooManyRequests`
- `HttpStatusCode.InternalServerError`
- `HttpStatusCode.BadGateway`
- `HttpStatusCode.ServiceUnavailable`

### **2. Circuit Breaker States**
- **Closed**: درخواست‌ها عادی
- **Open**: درخواست‌ها رد می‌شوند
- **Half-Open**: یک درخواست تست

### **3. Logging Levels**
- **Information**: General retries
- **Warning**: Auth retries, timeouts
- **Error**: Circuit breaker opened

## 📊 **Monitoring & Observability**

### **1. Structured Logging**
```csharp
_logger.LogWarning(
    "Auth retry {RetryAttempt} after {Delay}ms for {OperationKey}. " +
    "Status: {StatusCode}, Exception: {Exception}",
    retryAttempt, timespan.TotalMilliseconds, context.OperationKey,
    outcome.Result?.StatusCode, outcome.Exception?.Message);
```

### **2. Context Information**
- **OperationKey**: نام endpoint
- **RetryAttempt**: شماره retry
- **Delay**: زمان انتظار
- **StatusCode**: HTTP status code

## 🎯 **Use Cases**

### **1. Login Operation**
```csharp
// POST /api/auth/login
// Policy: Auth Policy
// Retry: 3 بار
// Circuit Breaker: 3 خطا = 30 ثانیه break
// Timeout: 30 ثانیه
```

### **2. User Profile Update**
```csharp
// PUT /api/users/profile
// Policy: General Policy
// Retry: 2 بار
// Circuit Breaker: 5 خطا = 60 ثانیه break
// Timeout: 30 ثانیه
```

### **3. Get User Data**
```csharp
// GET /api/users/{id}
// Policy: Read-Only Policy
// Retry: 2 بار
// Timeout: 30 ثانیه
```

## 🛡️ **Security Considerations**

### **1. Authentication Retry**
- Retry فقط برای transient errors
- Circuit breaker برای جلوگیری از brute force
- Timeout برای جلوگیری از hanging requests

### **2. Rate Limiting**
- `429 Too Many Requests` handle می‌شود
- Exponential backoff برای rate limiting
- Circuit breaker برای persistent rate limiting

## 🔄 **Integration with Existing Code**

### **1. AuthApiClient**
```csharp
public async Task<ApiResponse<AuthResultDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
{
    // Polly automatically handles retries and circuit breaker
    var result = await _httpClient.PostAsync<LoginDto, AuthResultDto>("api/Auth/login", dto, cancellationToken);
    // ...
}
```

### **2. UserSessionService**
```csharp
public async Task<ApiResponse<LogoutResultDto>> LogoutAsync(bool logoutFromAllDevices = false, CancellationToken cancellationToken = default)
{
    // Polly automatically handles retries and circuit breaker
    var backendResult = await _authApiClient.LogoutAsync(logoutDto, cancellationToken);
    // ...
}
```

## 📈 **Performance Benefits**

### **1. Reduced Downtime**
- Automatic retry برای transient failures
- Circuit breaker برای persistent failures
- Better user experience

### **2. Resource Management**
- Timeout برای جلوگیری از resource leaks
- Exponential backoff برای کاهش server load
- Circuit breaker برای جلوگیری از cascade failures

### **3. Monitoring**
- Detailed logging برای troubleshooting
- Metrics برای performance analysis
- Context information برای debugging

## 🚀 **Deployment Considerations**

### **1. Environment-Specific Settings**
```json
// Production
"RetryCount": 3,
"CircuitBreakerThreshold": 3

// Development
"RetryCount": 1,
"CircuitBreakerThreshold": 1
```

### **2. Monitoring Setup**
- Log aggregation (ELK Stack)
- Metrics collection (Prometheus)
- Alerting (Grafana)

### **3. Testing**
- Unit tests برای policies
- Integration tests برای HTTP client
- Load testing برای circuit breaker

## ✅ **Best Practices**

### **1. Policy Selection**
- Auth operations: Most resilient
- Write operations: Balanced
- Read operations: Lightweight

### **2. Configuration**
- Environment-specific settings
- Monitoring-friendly logging
- Performance-optimized timeouts

### **3. Error Handling**
- Graceful degradation
- User-friendly error messages
- Proper logging levels

## 🔧 **Troubleshooting**

### **1. Common Issues**
- Circuit breaker stuck open
- Excessive retries
- Timeout issues

### **2. Debugging**
- Check logs for policy events
- Monitor circuit breaker state
- Analyze retry patterns

### **3. Optimization**
- Adjust retry counts
- Tune circuit breaker thresholds
- Optimize timeout values 