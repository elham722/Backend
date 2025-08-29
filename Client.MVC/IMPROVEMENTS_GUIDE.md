# راهنمای بهبودهای کلی پروژه

## خلاصه بهبودهای اعمال شده

### 1. بهبود تنظیمات و پیکربندی (`appsettings.json`)

#### تغییرات اعمال شده:
- اضافه کردن بخش `ApiSettings` برای مدیریت بهتر تنظیمات API
- بهبود بخش `CookieSecurity` با اضافه کردن `ExpirationMinutes`
- اضافه کردن بخش `Security` برای کنترل هدرهای امنیتی

#### مزایا:
- مدیریت متمرکز تنظیمات
- قابلیت تغییر آسان تنظیمات بدون تغییر کد
- امنیت بهتر کوکی‌ها

### 2. بهبود Program.cs

#### تغییرات اعمال شده:
- اضافه کردن `ForwardedHeaders` برای پشتیبانی از پروکسی
- بهبود تنظیمات Session با امنیت بیشتر
- اضافه کردن هدرهای امنیتی (CSP, XSS Protection, Frame Options)
- اضافه کردن Health Checks
- بهبود Dependency Injection

#### مزایا:
- امنیت بیشتر
- پشتیبانی از محیط‌های مختلف
- مانیتورینگ بهتر

### 3. سرویس مدیریت خطا (`ErrorHandlingService`)

#### قابلیت‌ها:
- لاگینگ ساختاریافته خطاها
- فیلتر کردن خطاهای غیرضروری
- پاکسازی اطلاعات حساس از لاگ‌ها
- پشتیبانی از context و additional data

#### مزایا:
- مدیریت بهتر خطاها
- امنیت بیشتر در لاگینگ
- قابلیت ردیابی بهتر مشکلات

### 4. سرویس مدیریت کش (`CacheService`)

#### قابلیت‌ها:
- کش کردن با expiration time
- پشتیبانی از sliding expiration
- حذف بر اساس pattern
- آمارگیری از کش

#### مزایا:
- بهبود عملکرد
- کاهش بار سرور
- مدیریت بهتر حافظه

### 5. بهبود BaseController

#### قابلیت‌های جدید:
- `ExecuteWithErrorHandlingAsync`: اجرای عملیات با مدیریت خطا
- `GetCachedOrExecuteAsync`: کش کردن نتایج
- `LogUserActionAsync`: لاگ کردن فعالیت‌های کاربر
- `JsonError` و `JsonSuccess`: پاسخ‌های JSON استاندارد

#### مزایا:
- کد تمیزتر و قابل نگهداری
- مدیریت بهتر خطاها
- لاگینگ بهتر فعالیت‌ها

### 6. Global Exception Middleware

#### قابلیت‌ها:
- مدیریت سراسری خطاها
- لاگینگ خودکار خطاها
- پاسخ‌های JSON استاندارد
- تشخیص نوع خطا و کد وضعیت مناسب

#### مزایا:
- مدیریت متمرکز خطاها
- امنیت بیشتر
- تجربه کاربری بهتر

### 7. بهبود ErrorViewModel

#### فیلدهای جدید:
- `Message`: پیام خطا
- `ExceptionType`: نوع استثنا
- `Timestamp`: زمان خطا
- `UserAgent` و `IpAddress`: اطلاعات درخواست

#### مزایا:
- اطلاعات بیشتر برای دیباگ
- تجربه کاربری بهتر

## نحوه استفاده از بهبودها

### 1. استفاده از Error Handling در Controller

```csharp
public async Task<IActionResult> SomeAction()
{
    return await ExecuteWithErrorHandlingAsync(async () =>
    {
        // کد عملیات
        var result = await SomeOperation();
        return View(result);
    }, "SomeAction");
}
```

### 2. استفاده از کش

```csharp
public async Task<IActionResult> GetData()
{
    var data = await GetCachedOrExecuteAsync("user-data", async () =>
    {
        return await FetchDataFromApi();
    }, TimeSpan.FromMinutes(10));
    
    return View(data);
}
```

### 3. لاگ کردن فعالیت کاربر

```csharp
public async Task<IActionResult> UpdateProfile(UpdateProfileModel model)
{
    await LogUserActionAsync("UpdateProfile", new { model.Email });
    // کد عملیات
}
```

### 4. پاسخ‌های JSON

```csharp
public IActionResult ApiAction()
{
    if (IsAjaxRequest())
    {
        return JsonSuccess(data, "عملیات موفق");
    }
    return View();
}
```

## تنظیمات پیشنهادی برای محیط Production

### 1. appsettings.Production.json

```json
{
  "ApiSettings": {
    "BaseUrl": "https://your-production-api.com/",
    "TimeoutSeconds": 60,
    "MaxRetryAttempts": 5
  },
  "Security": {
    "EnableHsts": true,
    "EnableCsp": true,
    "EnableXssProtection": true,
    "EnableFrameOptions": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Client.MVC": "Information"
    }
  }
}
```

### 2. تنظیمات کش

```json
{
  "CacheSettings": {
    "DefaultExpirationMinutes": 60,
    "SlidingExpirationMinutes": 20
  }
}
```

## نکات مهم

1. **امنیت**: تمام هدرهای امنیتی فعال شده‌اند
2. **عملکرد**: کش و resilience policies برای بهبود عملکرد
3. **قابلیت نگهداری**: کد تمیز و قابل نگهداری
4. **مانیتورینگ**: لاگینگ کامل و health checks

## مراحل بعدی

1. تست کردن تمام قابلیت‌های جدید
2. تنظیم لاگینگ برای محیط production
3. مانیتورینگ عملکرد کش
4. بهینه‌سازی بیشتر بر اساس نیازها 