# راهنمای تست reCAPTCHA

## 🧪 نحوه تست

### 1️⃣ اجرای Backend API
```bash
cd Backend.Api
dotnet run
```

### 2️⃣ اجرای Client MVC
```bash
cd Client.MVC
dotnet run
```

### 3️⃣ تست صفحه Registration
1. بروید به: `https://localhost:7001/Auth/Register`
2. Console را باز کنید (F12)
3. فرم را پر کنید
4. روی دکمه **Register** کلیک کنید

## 🔍 مراحل اجرا

### مرحله 1: لود شدن reCAPTCHA
```
🚀 Loading reCAPTCHA v3...
✅ Google reCAPTCHA loaded
⚡ Initializing reCAPTCHA...
🎯 reCAPTCHA ready
✅ تأیید امنیتی آماده است
```

### مرحله 2: کلیک روی دکمه Register
```
📝 Register button clicked
🔄 Generating CAPTCHA...
🔄 Executing reCAPTCHA...
✅ reCAPTCHA success! Token length: 1847
✅ تأیید امنیتی انجام شد
```

### مرحله 3: ارسال فرم
- فرم به کنترلر ارسال می‌شود
- `CaptchaBehavior` اجرا می‌شود
- `GoogleRecaptchaService` token را اعتبارسنجی می‌کند
- اگر موفق بود، `RegisterCommandHandler` اجرا می‌شود

## 🐛 عیب‌یابی

### مشکل: فرم ارسال نمی‌شود

#### راه حل 1: بررسی Console
```javascript
// در Console این پیام‌ها را ببینید:
console.log('📝 Register button clicked');
console.log('🔄 Generating CAPTCHA...');
```

#### راه حل 2: بررسی reCAPTCHA
```javascript
// تست کنید:
testCaptcha();
```

#### راه حل 3: بررسی Network Tab
- Network tab را باز کنید
- روی دکمه Register کلیک کنید
- درخواست‌های HTTP را بررسی کنید

### مشکل: reCAPTCHA لود نمی‌شود

#### راه حل 1: بررسی Site Key
```javascript
// در Console:
console.log('Site Key:', '6Leu9bgrAAAAAK4CRoviNVfx160-mRf8HoF7x4yD');
```

#### راه حل 2: بررسی Script Loading
```javascript
// در Console:
console.log('grecaptcha exists:', typeof grecaptcha !== 'undefined');
```

#### راه حل 3: بررسی CORS
- در Network tab ببینید آیا script لود شده
- اگر خطای CORS دارید، دامنه را در Google reCAPTCHA اضافه کنید

## 🔧 تنظیمات Development

### Bypass در Development
```json
// appsettings.Development.json
{
  "Recaptcha": {
    "BypassInDevelopment": true
  }
}
```

**نکته**: در Development، CAPTCHA bypass می‌شود و همیشه موفق برمی‌گردد.

### فعال در Development
```json
// appsettings.Development.json
{
  "Recaptcha": {
    "BypassInDevelopment": false
  }
}
```

**نکته**: در این حالت، CAPTCHA واقعی اجرا می‌شود.

## 📊 تست‌های مختلف

### تست 1: Development Bypass
1. `BypassInDevelopment: true`
2. فرم را پر کنید
3. روی Register کلیک کنید
4. **نتیجه**: فرم بدون CAPTCHA ارسال می‌شود

### تست 2: Real CAPTCHA
1. `BypassInDevelopment: false`
2. فرم را پر کنید
3. روی Register کلیک کنید
4. **نتیجه**: reCAPTCHA اجرا می‌شود

### تست 3: Invalid Token
1. `CaptchaToken` را خالی بگذارید
2. فرم را ارسال کنید
3. **نتیجه**: خطای CAPTCHA validation

## 🚀 تست Production

### تنظیمات Production
```json
// appsettings.json
{
  "Recaptcha": {
    "Enabled": true,
    "Version": "V3",
    "SiteKey": "YOUR_PRODUCTION_SITE_KEY",
    "SecretKey": "YOUR_PRODUCTION_SECRET_KEY",
    "MinimumScore": 0.5,
    "BypassInDevelopment": false
  }
}
```

### نکات مهم
1. **دامنه واقعی** را در Google reCAPTCHA اضافه کنید
2. **کلیدهای Production** را استفاده کنید
3. **HTTPS** فعال باشد
4. **Score Threshold** مناسب تنظیم کنید

## 📝 لاگ‌ها

### Backend Logs
```
[INFO] Verifying CAPTCHA for request RegisterCommand
[INFO] reCAPTCHA verification successful. Score: 0.8, Action: register
[INFO] CAPTCHA verification successful for request RegisterCommand
```

### Frontend Logs
```
🚀 Loading reCAPTCHA v3...
✅ Google reCAPTCHA loaded
🎯 reCAPTCHA ready
📝 Register button clicked
🔄 Generating CAPTCHA...
✅ reCAPTCHA success! Token length: 1847
```

## 🎯 نتیجه‌گیری

با این پیاده‌سازی:

1. ✅ **reCAPTCHA v3** درست کار می‌کند
2. ✅ **Token های طولانی** طبیعی هستند
3. ✅ **Pipeline Behavior** CAPTCHA را اعتبارسنجی می‌کند
4. ✅ **Development Bypass** برای تست راحت
5. ✅ **Production Ready** برای استفاده واقعی

**اگر هنوز مشکل دارید، Console و Network tab را بررسی کنید!** 🔍 