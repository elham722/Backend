# راهنمای کامل Resilience Policies

## 🎯 **هدف**

این سیستم برای مدیریت خطاها و بهبود reliability درخواست‌های HTTP طراحی شده:

1. **Timeout Management** - جلوگیری از hanging requests
2. **Circuit Breaker** - جلوگیری از cascade failures
3. **Retry with Exponential Backoff** - مدیریت transient failures
4. **Policy Wrapping** - ترکیب policies برای بهترین نتیجه

## 🏗️ **معماری Policies**

### **1. Policy Order (مهم!)**
```csharp
// ✅ Correct Order: Timeout -> Circuit Breaker -> Retry
Policy.WrapAsync(
    CreateTimeoutPolicy(10),           // Outer layer
    CreateCircuitBreakerPolicy(),      // Middle layer  
    CreateRetryPolicy()                // Inner layer
);
```

**چرا این ترتیب مهمه؟**
- **Timeout**: اولین خط دفاع - اگر request طول بکشه، timeout می‌شه
- **Circuit Breaker**: اگر چندین request fail بشه، circuit باز می‌شه
- **Retry**: آخرین خط دفاع - retry می‌کنه تا موفق بشه

### **2. Policy Types**

#### **Auth Policy (Most Resilient)**
```csharp
// برای عملیات حساس: Login, Register, Refresh Token
- Timeout: 10 ثانیه
- Circuit Breaker: 3 خطا = 30 ثانیه break
- Retry: 3 بار با exponential backoff
```

#### **General Policy (Balanced)**
```csharp
// برای عملیات معمولی: PUT, DELETE, PATCH
- Timeout: 10 ثانیه
- Circuit Breaker: 5 خطا = 60 ثانیه break
- Retry: 2 بار با exponential backoff
```

#### **Read-Only Policy (Lightweight)**
```csharp
// برای عملیات خواندن: GET
- Timeout: 10 ثانیه
- Circuit Breaker: 3 خطا = 30 ثانیه break
- Retry: 2 بار با exponential backoff
```

## ⚙️ **Configuration**

### **appsettings.json**
```json
{
  "ResiliencePolicies": {
    "Auth": {
      "RetryCount": 3,
      "RetryDelaySeconds": 2,
      "CircuitBreakerThreshold": 3,
      "CircuitBreakerDurationSeconds": 30,
      "TimeoutSeconds": 10
    },
    "General": {
      "RetryCount": 2,
      "RetryDelaySeconds": 1,
      "CircuitBreakerThreshold": 5,
      "CircuitBreakerDurationSeconds": 60,
      "TimeoutSeconds": 10
    },
    "ReadOnly": {
      "RetryCount": 2,
      "RetryDelaySeconds": 1,
      "CircuitBreakerThreshold": 3,
      "CircuitBreakerDurationSeconds": 30,
      "TimeoutSeconds": 10
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
      "TimeoutSeconds": 5
    }
  }
}
```

## 🔧 **Implementation Details**

### **1. Timeout Policy**
```csharp
public AsyncTimeoutPolicy<HttpResponseMessage> CreateTimeoutPolicy(int timeoutSeconds)
{
    return Policy.TimeoutAsync<HttpResponseMessage>(
        timeout: TimeSpan.FromSeconds(timeoutSeconds),
        timeoutStrategy: TimeoutStrategy.Optimistic, // مهم!
        onTimeoutAsync: (context, timespan, task) =>
        {
            _logger.LogWarning("Request timeout after {Timeout}ms for {OperationKey}", 
                timespan.TotalMilliseconds, context.OperationKey);
            return Task.CompletedTask;
        });
}
```

**نکات مهم:**
- **Optimistic Strategy**: برای HTTP requests بهتره
- **10 ثانیه**: تعادل بین user experience و reliability
- **Logging**: برای monitoring و debugging

### **2. Circuit Breaker Policy**
```csharp
public AsyncCircuitBreakerPolicy<HttpResponseMessage> CreateAuthCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
        .Or<SocketException>()
        .Or<HttpRequestException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, timespan) => { /* Log error */ },
            onReset: () => { /* Log reset */ },
            onHalfOpen: () => { /* Log half-open */ });
}
```

**نکات مهم:**
- **Threshold**: 3-5 خطا قبل از باز شدن circuit
- **Duration**: 30-60 ثانیه break duration
- **States**: Closed -> Open -> Half-Open -> Closed

### **3. Retry Policy**
```csharp
public AsyncRetryPolicy<HttpResponseMessage> CreateAuthRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
        .Or<SocketException>()
        .Or<HttpRequestException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(2 * Math.Pow(2, retryAttempt - 1)), // Exponential backoff
            onRetry: (outcome, timespan, retryAttempt, context) => { /* Log retry */ });
}
```

**نکات مهم:**
- **Exponential Backoff**: 2s, 4s, 8s
- **Transient Errors**: 5xx, 429, SocketException
- **Logging**: برای monitoring retry attempts

## 🚀 **Usage in AuthenticatedHttpClient**

### **1. Automatic Policy Selection**
```csharp
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
    var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
    httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
    AddCustomHeadersToRequest(httpRequest);
    var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
    
    // Handle authentication errors (existing logic)
    if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
        httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
    {
        var handled = await _authInterceptor.HandleAuthenticationErrorAsync(httpResponse);
        if (handled)
        {
            // Retry with new token
            httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            httpRequest = await _authInterceptor.AddAuthenticationHeaderAsync(httpRequest);
            AddCustomHeadersToRequest(httpRequest);
            httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
        }
    }
    return httpResponse;
}, new Context(endpoint));
```

## 📊 **Monitoring & Observability**

### **1. Logging Levels**
```csharp
// Information: Normal operations
_logger.LogInformation("Circuit breaker reset");

// Warning: Retries, timeouts, circuit breaker opened
_logger.LogWarning("Auth retry {RetryAttempt} after {Delay}ms");

// Error: Circuit breaker opened
_logger.LogError("Auth circuit breaker opened for {Duration}ms");
```

### **2. Context Information**
```csharp
// OperationKey: نام endpoint
// RetryAttempt: شماره retry
// Delay: زمان انتظار
// Duration: مدت زمان circuit breaker
// StatusCode: HTTP status code
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

## 🔧 **Troubleshooting**

### **1. Common Issues**
```csharp
// Issue: Client hangs indefinitely
// Solution: Add timeout policy

// Issue: Cascade failures
// Solution: Add circuit breaker

// Issue: Excessive retries
// Solution: Reduce retry count or add circuit breaker

// Issue: Slow responses
// Solution: Reduce timeout or retry delay
```

### **2. Debugging**
```csharp
// Check logs for policy events
// Monitor circuit breaker state
// Analyze retry patterns
// Check timeout occurrences
```

## ✅ **Best Practices**

### **1. Policy Selection**
- **Auth operations**: Most resilient (3 retries, 3 circuit breaker)
- **Write operations**: Balanced (2 retries, 5 circuit breaker)
- **Read operations**: Lightweight (2 retries, 3 circuit breaker)

### **2. Configuration**
- **Development**: Lower values for faster feedback
- **Production**: Higher values for stability
- **Monitoring**: Log all policy events

### **3. Error Handling**
- **Graceful degradation**: Circuit breaker prevents cascade failures
- **User experience**: Timeout prevents hanging
- **Resource management**: Exponential backoff reduces server load

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

این سیستم تضمین می‌کند که application شما در برابر network failures و server issues مقاوم باشد. 