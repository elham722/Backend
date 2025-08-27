# راهنمای تنظیم Swagger

## تنظیمات فعلی

### 1. فایل launchSettings.json
فایل `Backend.Api/Properties/launchSettings.json` تنظیم شده تا:

- **launchBrowser**: `true` - مرورگر به صورت خودکار باز می‌شود
- **launchUrl**: `swagger` - مستقیماً به صفحه Swagger می‌رود
- **applicationUrl**: آدرس‌های HTTP و HTTPS تنظیم شده

### 2. Profile های موجود

#### HTTP Profile
```json
"http": {
  "launchBrowser": true,
  "launchUrl": "swagger",
  "applicationUrl": "http://localhost:5187"
}
```

#### HTTPS Profile
```json
"https": {
  "launchBrowser": true,
  "launchUrl": "swagger",
  "applicationUrl": "https://localhost:7209;http://localhost:5187"
}
```

#### Swagger Profile (مخصوص)
```json
"Swagger": {
  "launchBrowser": true,
  "launchUrl": "swagger",
  "applicationUrl": "https://localhost:7209;http://localhost:5187"
}
```

## نحوه استفاده

### 1. اجرا از Visual Studio
- پروژه را اجرا کنید (F5 یا Ctrl+F5)
- مرورگر به صورت خودکار باز می‌شود و به آدرس Swagger می‌رود
- آدرس: `https://localhost:7209/swagger` یا `http://localhost:5187/swagger`

### 2. اجرا از Command Line
```bash
# اجرا با HTTPS
dotnet run --launch-profile https

# اجرا با HTTP
dotnet run --launch-profile http

# اجرا با Swagger profile
dotnet run --launch-profile Swagger
```

### 3. تغییر Profile پیش‌فرض
اگر می‌خواهید profile خاصی به عنوان پیش‌فرض تنظیم شود:

1. در Visual Studio: روی پروژه راست کلیک کنید
2. Properties را انتخاب کنید
3. در تب Debug، Profile مورد نظر را انتخاب کنید

## آدرس‌های مفید

### Swagger UI
- **HTTPS**: `https://localhost:7209/swagger`
- **HTTP**: `http://localhost:5187/swagger`

### API Endpoints
- **Auth Register**: `POST /api/auth/register`
- **Swagger JSON**: `/swagger/v1/swagger.json`

## تست API

### 1. از طریق Swagger UI
1. پروژه را اجرا کنید
2. در Swagger UI، endpoint `/api/auth/register` را پیدا کنید
3. روی "Try it out" کلیک کنید
4. اطلاعات را وارد کنید:

```json
{
  "email": "test@example.com",
  "userName": "testuser",
  "password": "Test123!@#",
  "confirmPassword": "Test123!@#",
  "phoneNumber": "+1234567890",
  "acceptTerms": true,
  "subscribeToNewsletter": false
}
```

### 2. از طریق HTTP Client
از فایل `Backend.Api/Backend.Api.http` استفاده کنید:

```http
### Register a new user
POST {{baseUrl}}/api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "userName": "testuser",
  "password": "Test123!@#",
  "confirmPassword": "Test123!@#",
  "phoneNumber": "+1234567890",
  "acceptTerms": true,
  "subscribeToNewsletter": false
}
```

## تنظیمات اضافی

### 1. تغییر آدرس Swagger
اگر می‌خواهید آدرس Swagger را تغییر دهید:

```csharp
// در Program.cs
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend API V1");
    c.RoutePrefix = "api-docs"; // تغییر آدرس به /api-docs
});
```

### 2. اضافه کردن توضیحات بیشتر
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Backend API", 
        Version = "v1",
        Description = "API for Backend Application"
    });
});
```

## نکات مهم

1. **Environment**: Swagger فقط در Development environment فعال است
2. **Security**: در Production، Swagger غیرفعال می‌شود
3. **CORS**: اگر از frontend جداگانه استفاده می‌کنید، CORS را تنظیم کنید
4. **Authentication**: برای endpoints محافظت شده، JWT token را در Swagger تنظیم کنید 