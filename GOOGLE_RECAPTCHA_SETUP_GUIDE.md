# راهنمای راه‌اندازی Google reCAPTCHA

## مقدمه

این راهنما نحوه راه‌اندازی و استفاده از Google reCAPTCHA در پروژه Backend را توضیح می‌دهد.

## مراحل راه‌اندازی

### 1. دریافت کلیدهای Google reCAPTCHA

1. به [Google reCAPTCHA Admin Console](https://www.google.com/recaptcha/admin) بروید
2. روی "Create" کلیک کنید
3. نوع reCAPTCHA را انتخاب کنید:
   - **reCAPTCHA v3**: برای تشخیص خودکار (توصیه شده)
   - **reCAPTCHA v2**: برای چک‌باکس یا چالش تصویری
4. دامنه‌های خود را اضافه کنید
5. کلیدهای Site Key و Secret Key را کپی کنید

### 2. تنظیم فایل appsettings.json

در فایل `Backend.Api/appsettings.json` بخش Captcha را به‌روزرسانی کنید:

```json
{
  "Captcha": {
    "IsEnabled": true,
    "Type": "GoogleReCaptcha",
    "Action": "register",
    "RequireForAllRequests": false,
    "ExemptIpAddresses": ["127.0.0.1", "::1"],
    "RateLimit": {
      "MaxFailedAttempts": 5,
      "TimeWindowMinutes": 15,
      "BlockIpAfterLimit": true,
      "BlockDurationMinutes": 60
    },
    "EnableLogging": true,
    "GoogleReCaptcha": {
      "IsEnabled": true,
      "SiteKey": "YOUR_ACTUAL_SITE_KEY_HERE",
      "SecretKey": "YOUR_ACTUAL_SECRET_KEY_HERE",
      "ScoreThreshold": 0.5,
      "ApiEndpoint": "https://www.google.com/recaptcha/api/siteverify",
      "TimeoutSeconds": 10,
      "UseProxy": false
    }
  }
}
```

**مهم**: کلیدهای `SiteKey` و `SecretKey` را با کلیدهای واقعی خود جایگزین کنید.

### 3. تنظیمات Score Threshold

- **0.0**: احتمال بالای bot بودن
- **1.0**: احتمال بالای انسان بودن
- **0.5**: حد متوسط (توصیه شده)
- **0.7**: حد سختگیرانه‌تر

### 4. اضافه کردن reCAPTCHA به صفحات

#### در HTML:

```html
<!-- در head صفحه -->
<script src="~/js/captcha.js"></script>
<script src="~/js/captcha-integration.js"></script>

<!-- در فرم -->
<div id="captchaContainer">
    <button type="button" id="executeCaptcha" class="btn btn-primary">
        اجرای CAPTCHA
    </button>
    <input type="hidden" id="captchaToken" name="CaptchaToken" />
</div>
```

#### در JavaScript:

```javascript
// اجرای CAPTCHA
async function executeCaptcha() {
    try {
        const token = await captchaService.execute('register');
        console.log('CAPTCHA token:', token);
        return token;
    } catch (error) {
        console.error('CAPTCHA failed:', error);
    }
}

// اعتبارسنجی CAPTCHA
async function validateCaptcha(token) {
    try {
        const result = await captchaService.validate(token, 'register');
        if (result.isValid) {
            console.log('CAPTCHA validation successful');
            return true;
        } else {
            console.error('CAPTCHA validation failed:', result.errorMessage);
            return false;
        }
    } catch (error) {
        console.error('CAPTCHA validation error:', error);
        return false;
    }
}
```

## API Endpoints

### 1. دریافت تنظیمات CAPTCHA

```
GET /api/captcha/config
```

**Response:**
```json
{
  "siteKey": "your-site-key",
  "action": "recaptcha",
  "threshold": 0.5,
  "isEnabled": true,
  "type": "GoogleReCaptcha"
}
```

### 2. اعتبارسنجی reCAPTCHA

```
POST /api/captcha/validate-google
```

**Request Body:**
```json
{
  "token": "recaptcha-token",
  "action": "register",
  "ipAddress": "optional-client-ip"
}
```

**Response:**
```json
{
  "isValid": true,
  "score": 0.8,
  "action": "register",
  "challengePassed": "PASSED",
  "timestamp": "2024-01-01T00:00:00Z",
  "errorMessage": null
}
```

## ویژگی‌های امنیتی

### 1. Rate Limiting

- حداکثر 5 تلاش ناموفق در 15 دقیقه
- مسدود کردن IP پس از تجاوز از حد
- مدت زمان مسدودیت: 60 دقیقه

### 2. IP Exemption

IP های محلی از CAPTCHA معاف هستند:
- `127.0.0.1` (localhost)
- `::1` (IPv6 localhost)

### 3. Logging

تمام تلاش‌های CAPTCHA ثبت می‌شوند:
- IP آدرس
- نتیجه اعتبارسنجی
- امتیاز reCAPTCHA
- زمان تلاش

## عیب‌یابی

### مشکلات رایج

#### 1. "CAPTCHA service is not available"

**علت:** سرویس CAPTCHA درست راه‌اندازی نشده
**راه‌حل:** 
- بررسی تنظیمات `appsettings.json`
- بررسی Dependency Injection
- بررسی لاگ‌های سرور

#### 2. "Failed to load reCAPTCHA script"

**علت:** مشکل در بارگذاری اسکریپت Google
**راه‌حل:**
- بررسی اتصال اینترنت
- بررسی فیلترینگ
- استفاده از CDN جایگزین

#### 3. "Score below threshold"

**علت:** امتیاز reCAPTCHA کمتر از حد مجاز
**راه‌حل:**
- کاهش `ScoreThreshold` در تنظیمات
- بررسی رفتار کاربر (ممکن است واقعاً bot باشد)

### بررسی لاگ‌ها

```bash
# بررسی لاگ‌های CAPTCHA
tail -f logs/captcha.log

# بررسی لاگ‌های عمومی
tail -f logs/application.log
```

## تست

### 1. تست محلی

```bash
# اجرای پروژه
dotnet run --project Backend.Api

# تست API
curl -X GET "https://localhost:5001/api/captcha/config"
```

### 2. تست reCAPTCHA

1. صفحه ثبت‌نام را باز کنید
2. روی دکمه "اجرای CAPTCHA" کلیک کنید
3. منتظر بمانید تا reCAPTCHA اجرا شود
4. فرم را ارسال کنید
5. نتیجه را در کنسول مرورگر بررسی کنید

## نکات مهم

### 1. امنیت

- **هرگز** Secret Key را در کد frontend قرار ندهید
- از HTTPS استفاده کنید
- Rate Limiting را فعال نگه دارید

### 2. عملکرد

- reCAPTCHA v3 نامرئی است و تجربه کاربری را خراب نمی‌کند
- امتیاز threshold را خیلی بالا نگذارید
- از CDN Google استفاده کنید

### 3. پشتیبانی

- در صورت مشکل با Google reCAPTCHA تماس بگیرید
- لاگ‌های سرور را بررسی کنید
- تنظیمات را دوباره بررسی کنید

## منابع مفید

- [Google reCAPTCHA Documentation](https://developers.google.com/recaptcha)
- [reCAPTCHA Admin Console](https://www.google.com/recaptcha/admin)
- [reCAPTCHA v3 Demo](https://www.google.com/recaptcha/api2/demo)
- [reCAPTCHA Best Practices](https://developers.google.com/recaptcha/docs/faq)

## پشتیبانی

در صورت بروز مشکل یا نیاز به راهنمایی بیشتر، با تیم توسعه تماس بگیرید. 