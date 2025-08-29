# راهنمای بهینه‌سازی Resilience Policies

## خلاصه پیاده‌سازی

✅ **Timeout و Circuit Breaker پیاده‌سازی شده‌اند:**

### 🔧 **تنظیمات Auth Operations (Login/Refresh)**

```json
"Auth": {
  "RetryCount": 2,
  "RetryDelaySeconds": 1,
  "CircuitBreakerThreshold": 2,
  "CircuitBreakerDurationSeconds": 15,
  "TimeoutSeconds": 8
}
```

### 🚀 **تنظیمات Critical Auth Operations (Refresh Token)**

```csharp
// Timeout: 5 seconds
// Circuit Breaker: 1 failure threshold, 10 seconds break
// Retry: 1 attempt with 0.5 seconds delay
var criticalPolicy = CreateCriticalAuthPolicy();
```

## ویژگی‌های پیاده‌سازی شده

### 1. **Timeout Policies**
- ✅ **Auth Operations**: 8 ثانیه timeout
- ✅ **Critical Auth**: 5 ثانیه timeout (برای refresh token)
- ✅ **General Operations**: 15 ثانیه timeout
- ✅ **Read-Only Operations**: 12 ثانیه timeout

### 2. **Circuit Breaker Policies**
- ✅ **Auth Operations**: 2 شکست برای باز شدن circuit
- ✅ **Critical Auth**: 1 شکست برای باز شدن circuit (سریع‌تر)
- ✅ **General Operations**: 5 شکست برای باز شدن circuit
- ✅ **Read-Only Operations**: 3 شکست برای باز شدن circuit

### 3. **Retry Policies**
- ✅ **Auth Operations**: 2 retry با linear backoff (1s, 2s)
- ✅ **Critical Auth**: 1 retry با delay کوتاه (0.5s)
- ✅ **General Operations**: 3 retry با exponential backoff
- ✅ **Read-Only Operations**: 2 retry با exponential backoff

## مزایای پیاده‌سازی

### 🎯 **برای Auth Operations:**
- **Timeout کوتاه‌تر**: 8 ثانیه برای جلوگیری از انتظار طولانی
- **Circuit Breaker سریع**: 2 شکست برای باز شدن سریع
- **Retry کوتاه**: Linear backoff برای پاسخ سریع‌تر

### 🚀 **برای Critical Auth (Refresh Token):**
- **Timeout بسیار کوتاه**: 5 ثانیه
- **Circuit Breaker بسیار سریع**: 1 شکست
- **Retry محدود**: فقط 1 retry با delay کوتاه

### 📊 **برای General Operations:**
- **Timeout متوسط**: 15 ثانیه برای عملیات سنگین
- **Circuit Breaker محافظه‌کارانه**: 5 شکست
- **Retry بیشتر**: 3 retry برای عملیات مهم

## نحوه استفاده

### 1. **در AuthenticationInterceptor**
```csharp
// برای refresh token از critical policy استفاده می‌شود
var policy = _resiliencePolicyService.CreateCriticalAuthPolicy();
```

### 2. **در TokenManager**
```csharp
// برای refresh token از critical policy استفاده می‌شود
var policy = _resiliencePolicyService.CreateCriticalAuthPolicy();
```

### 3. **در AuthenticatedHttpClient**
```csharp
// برای auth operations از auth policy استفاده می‌شود
var policy = isAuthOperation 
    ? _resiliencePolicyService.CreateAuthPolicy() 
    : _resiliencePolicyService.CreateGeneralPolicy();
```

## تنظیمات پیشنهادی برای Production

### 🔒 **Auth Operations (Production)**
```json
"Auth": {
  "RetryCount": 1,
  "RetryDelaySeconds": 0.5,
  "CircuitBreakerThreshold": 1,
  "CircuitBreakerDurationSeconds": 10,
  "TimeoutSeconds": 5
}
```

### 🚀 **Critical Auth (Production)**
```json
"CriticalAuth": {
  "RetryCount": 0,
  "RetryDelaySeconds": 0,
  "CircuitBreakerThreshold": 1,
  "CircuitBreakerDurationSeconds": 5,
  "TimeoutSeconds": 3
}
```

## مانیتورینگ و Logging

### 📊 **Logs برای Auth Operations:**
```
Auth retry 1 after 1000ms for login. Status: 500, Exception: Connection timeout
Auth circuit breaker opened for 15000ms. Status: 500, Exception: Server error
```

### 🚨 **Logs برای Critical Auth:**
```
Critical auth retry 1 after 500ms for refresh-token. Status: 401, Exception: Token expired
Critical auth circuit breaker opened for 10000ms. Status: 500, Exception: Server error
```

## نکات مهم

### ⚠️ **برای Auth Operations:**
1. **Timeout کوتاه**: جلوگیری از انتظار طولانی کاربر
2. **Circuit Breaker سریع**: جلوگیری از overload سرور
3. **Retry محدود**: جلوگیری از spam درخواست‌ها

### 🎯 **برای Refresh Token:**
1. **Timeout بسیار کوتاه**: 5 ثانیه حداکثر
2. **Circuit Breaker بسیار سریع**: 1 شکست
3. **Retry محدود**: فقط 1 retry

### 📈 **برای General Operations:**
1. **Timeout متوسط**: 15 ثانیه برای عملیات سنگین
2. **Circuit Breaker محافظه‌کارانه**: 5 شکست
3. **Retry بیشتر**: 3 retry برای عملیات مهم

## نتیجه‌گیری

✅ **تمام الزامات برآورده شده‌اند:**

- ✅ **Timeout Policies**: برای همه عملیات پیاده‌سازی شده
- ✅ **Circuit Breaker**: برای همه عملیات پیاده‌سازی شده
- ✅ **Auth Operations**: تنظیمات بهینه با retry کوتاه‌تر
- ✅ **Critical Auth**: تنظیمات بسیار بهینه برای refresh token
- ✅ **Backoff Strategy**: Linear برای auth، Exponential برای general
- ✅ **Circuit Breaker Threshold**: کم برای auth (2)، متوسط برای general (5)

این پیاده‌سازی تضمین می‌کند که:
- 🔒 **امنیت**: Auth operations سریع و ایمن
- 🚀 **عملکرد**: Timeout و retry بهینه
- 🛡️ **مقاومت**: Circuit breaker برای جلوگیری از cascade failure
- 📊 **قابلیت نگهداری**: Logging کامل برای monitoring 