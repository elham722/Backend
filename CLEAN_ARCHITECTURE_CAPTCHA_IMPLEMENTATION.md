# پیاده‌سازی Clean Architecture برای reCAPTCHA

## 🎯 هدف

پیاده‌سازی reCAPTCHA طبق اصول Clean Architecture که:
- **تمیز و قابل نگهداری** باشد
- **قابل تست** باشد  
- **قابل توسعه** باشد
- **برای پروداکشن آماده** باشد

## 🏗️ ساختار Clean Architecture

### 1️⃣ لایه Application (قراردادها)

#### IHumanVerificationService
```csharp
// Backend.Application/Common/Security/IHumanVerificationService.cs
public interface IHumanVerificationService
{
    Task<CaptchaVerificationResult> VerifyAsync(
        string token, 
        string? action = null, 
        string? userIp = null, 
        CancellationToken ct = default);
}
```

#### IRequireCaptcha (مارکر)
```csharp
// Backend.Application/Common/Security/IRequireCaptcha.cs
public interface IRequireCaptcha
{
    string CaptchaToken { get; }
    string? CaptchaAction => null;
}
```

#### CaptchaBehavior (Pipeline Behavior)
```csharp
// Backend.Application/Common/Behaviors/CaptchaBehavior.cs
public class CaptchaBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    // CAPTCHA validation قبل از Handler اجرا می‌شود
}
```

### 2️⃣ لایه Infrastructure (پیاده‌سازی)

#### RecaptchaOptions
```csharp
// Backend.Infrastructure/Security/Recaptcha/RecaptchaOptions.cs
public sealed class RecaptchaOptions
{
    public bool Enabled { get; set; } = true;
    public RecaptchaVersion Version { get; set; } = RecaptchaVersion.V2Invisible;
    public string SiteKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public double MinimumScore { get; set; } = 0.5;
    public bool BypassInDevelopment { get; set; } = true;
}
```

#### GoogleRecaptchaService
```csharp
// Backend.Infrastructure/Security/Recaptcha/GoogleRecaptchaService.cs
internal sealed class GoogleRecaptchaService : IHumanVerificationService
{
    // پیاده‌سازی Google reCAPTCHA
    // پشتیبانی از v2 و v3
    // Bypass در محیط Development
}
```

### 3️⃣ لایه API (اتصال)

#### CaptchaController
```csharp
// Backend.Api/Controllers/CaptchaController.cs
[ApiController]
[Route("api/[controller]")]
public class CaptchaController : ControllerBase
{
    [HttpGet("config")]        // تنظیمات کپچا
    [HttpPost("validate")]     // اعتبارسنجی
}
```

## 🔄 جریان اجرا

### 1. درخواست Registration
```
User → RegisterCommand → CaptchaBehavior → GoogleRecaptchaService → Google API
```

### 2. Pipeline Behavior
```csharp
// 1. بررسی نیاز به کپچا
if (request is not IRequireCaptcha rc) 
    return await next();

// 2. اعتبارسنجی کپچا
var result = await _captchaService.VerifyAsync(...);

// 3. اگر موفق بود، ادامه
if (!result.Success)
    return Failure("CAPTCHA_FAILED");

return await next();
```

### 3. اعتبارسنجی در Google
```csharp
// ارسال به Google reCAPTCHA
var response = await _http.PostAsync("https://www.google.com/recaptcha/api/siteverify", form);

// بررسی پاسخ
bool success = root.GetProperty("success").GetBoolean();
double? score = root.TryGetProperty("score", out var sc) ? sc.GetDouble() : null;
```

## ⚙️ تنظیمات

### appsettings.json
```json
{
  "Recaptcha": {
    "Enabled": true,
    "Version": "V2Invisible",
    "SiteKey": "YOUR_SITE_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "MinimumScore": 0.5,
    "BypassInDevelopment": false
  }
}
```

### appsettings.Development.json
```json
{
  "Recaptcha": {
    "BypassInDevelopment": true
  }
}
```

## 🎨 UI Implementation

### reCAPTCHA v2 Invisible
```html
<button type="submit" 
        class="btn btn-primary g-recaptcha" 
        data-sitekey="YOUR_SITE_KEY"
        data-callback="onCaptchaSuccess"
        data-action="register">
    ثبت‌نام
</button>

<script src="https://www.google.com/recaptcha/api.js" async defer></script>
<script>
function onCaptchaSuccess(token) {
    document.getElementById("CaptchaToken").value = token;
    document.getElementById("register-form").submit();
}
</script>
```

## 🧪 تست

### 1. Development Environment
```bash
# Bypass در Development
"BypassInDevelopment": true
```

### 2. Production Environment
```bash
# فعال در Production
"BypassInDevelopment": false
```

### 3. Unit Tests
```csharp
// Mock IHumanVerificationService
services.AddScoped<IHumanVerificationService>(provider => 
    new MockHumanVerificationService { Success = true });
```

## 📊 مزایای Clean Architecture

### ✅ جداسازی مسئولیت‌ها
- **Application**: قراردادها و منطق کسب‌وکار
- **Infrastructure**: پیاده‌سازی سرویس‌های خارجی
- **API**: اتصال و کنترل‌کننده‌ها

### ✅ قابلیت تست
- **Unit Tests**: هر لایه جداگانه قابل تست
- **Integration Tests**: تست اتصال بین لایه‌ها
- **Mock Services**: جایگزینی سرویس‌های خارجی

### ✅ قابلیت توسعه
- **سرویس‌های جدید**: اضافه کردن بدون تغییر لایه‌های دیگر
- **تغییر Provider**: تغییر Google reCAPTCHA به سرویس دیگر
- **تنظیمات**: تغییر رفتار بدون تغییر کد

### ✅ امنیت
- **Pipeline Behavior**: اعتبارسنجی خودکار
- **Validation**: بررسی قبل از اجرای Handler
- **Logging**: ثبت تمام عملیات امنیتی

## 🚀 مراحل بعدی

### 1. اضافه کردن کامندهای دیگر
```csharp
public class LoginCommand : ICommand<Result<AuthResultDto>>, IRequireCaptcha
{
    public string CaptchaToken { get; set; } = string.Empty;
    public string? CaptchaAction { get; set; } = "login";
}
```

### 2. اضافه کردن Rate Limiting
```csharp
// برای جلوگیری از حملات
services.AddRateLimiter(options => {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter("GlobalLimiter",
            partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

### 3. اضافه کردن Metrics
```csharp
// برای مانیتورینگ
services.AddMetrics();
services.AddHealthChecks()
    .AddCheck<RecaptchaHealthCheck>("recaptcha");
```

## 🏆 نتیجه

با این پیاده‌سازی:

1. **کد تمیز و قابل نگهداری** است
2. **قابل تست** است
3. **قابل توسعه** است
4. **برای پروداکشن آماده** است
5. **طبق اصول Clean Architecture** است

**reCAPTCHA حالا بخشی از معماری سیستم است، نه یک add-on ساده!** ✨ 