# راهنمای پیاده‌سازی سیستم Register

## خلاصه پیاده‌سازی

این سیستم register با استفاده از معماری Clean Architecture و CQRS pattern پیاده‌سازی شده است.

## ساختار فایل‌ها

### Backend API
```
Backend.Api/
├── Controllers/
│   └── AuthController.cs          # API endpoints برای register و login
```

### Backend Application
```
Backend.Application/
├── Features/
│   └── UserManagement/
│       ├── Commands/
│       │   ├── Register/
│       │   │   ├── RegisterCommand.cs
│       │   │   ├── RegisterCommandHandler.cs
│       │   │   └── RegisterCommandValidator.cs
│       │   └── Login/
│       │       ├── LoginCommand.cs
│       │       ├── LoginCommandHandler.cs
│       │       └── LoginCommandValidator.cs
│       └── DTOs/
│           ├── AuthResultDto.cs
│           ├── RegisterDto.cs
│           └── LoginDto.cs
```

### Client MVC
```
Client.MVC/
├── Controllers/
│   └── AuthController.cs          # Controller برای صفحات auth
├── Services/
│   ├── IAuthApiClient.cs          # Interface برای Auth API client
│   └── AuthApiClient.cs           # Implementation برای Auth API client
├── Views/
│   └── Auth/
│       ├── Register.cshtml        # صفحه ثبت نام
│       └── Login.cshtml           # صفحه ورود
└── wwwroot/
    ├── css/
    │   └── auth.css               # استایل‌های صفحات auth
    └── js/
        └── auth.js                # JavaScript برای بهبود UX
```

## نحوه کارکرد

### 1. جریان Register
```
User → Client MVC → AuthController → ExternalService → Backend API → AuthController → RegisterCommand → RegisterCommandHandler → UserService → Database
```

### 2. جریان Login
```
User → Client MVC → AuthController → ExternalService → Backend API → AuthController → LoginCommand → LoginCommandHandler → UserService → Database
```

## ویژگی‌های پیاده‌سازی شده

### ✅ Backend Features
- [x] API endpoints برای register و login
- [x] CQRS pattern با MediatR
- [x] Validation با FluentValidation
- [x] Error handling و logging
- [x] Security features (IP tracking, User-Agent)
- [x] JWT token generation
- [x] استفاده از DTO های موجود در Application layer

### ✅ Frontend Features
- [x] Responsive design
- [x] Client-side validation
- [x] Password strength indicator
- [x] Loading states
- [x] Error handling
- [x] Session management
- [x] Persian/Farsi support
- [x] Type-safe communication با API
- [x] Typed API clients (AuthApiClient)
- [x] Dependency Inversion Principle

### ✅ Security Features
- [x] Password hashing
- [x] CSRF protection
- [x] Input validation
- [x] Rate limiting (قابل اضافه)
- [x] Session management
- [x] JWT token management

## نحوه اجرا

### 1. اجرای Backend API
```bash
cd Backend.Api
dotnet run
```

### 2. اجرای Client MVC
```bash
cd Client.MVC
dotnet run
```

### 3. دسترسی به صفحات
- ثبت نام: `https://localhost:5001/Auth/Register`
- ورود: `https://localhost:5001/Auth/Login`

## API Endpoints

### Register
```
POST /api/Auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "userName": "username",
  "password": "password123",
  "confirmPassword": "password123",
  "phoneNumber": "09123456789",
  "acceptTerms": true,
  "subscribeToNewsletter": false
}
```

**Response:**
```json
{
  "isSuccess": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "guid-here",
    "email": "user@example.com",
    "userName": "username",
    "phoneNumber": "09123456789",
    "emailConfirmed": false,
    "isActive": true
  }
}
```

### Login
```
POST /api/Auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "rememberMe": false
}
```

**Response:**
```json
{
  "isSuccess": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "guid-here",
    "email": "user@example.com",
    "userName": "username",
    "phoneNumber": "09123456789",
    "emailConfirmed": true,
    "isActive": true,
    "lastLoginAt": "2024-01-01T11:00:00Z"
  }
}
```

## Validation Rules

### Register Validation
- Email: Required, Valid email format
- UserName: Required, 3-50 characters
- Password: Required, minimum 6 characters
- ConfirmPassword: Required, must match Password
- PhoneNumber: Optional, valid phone format
- AcceptTerms: Required, must be true

### Login Validation
- Email: Required, Valid email format
- Password: Required

## Error Handling

### Backend Errors
- Validation errors با جزئیات
- Business rule violations
- Database errors
- Network errors

### Frontend Errors
- Client-side validation
- Server error display
- User-friendly error messages
- Auto-hide alerts

## Security Considerations

### Implemented
- Password hashing با BCrypt
- JWT token authentication
- Session management
- Input sanitization
- CSRF protection

### Recommended Additions
- Rate limiting
- Two-factor authentication
- Email verification
- Password reset functionality
- Account lockout after failed attempts

## Customization

### Styling
فایل `auth.css` را برای تغییر ظاهر ویرایش کنید.

### Validation
فایل‌های `*Validator.cs` را برای تغییر قوانین validation ویرایش کنید.

### Business Logic
فایل‌های `*Handler.cs` را برای تغییر منطق business ویرایش کنید.

### DTOs
فایل‌های DTO در `Backend.Application/Features/UserManagement/DTOs/` را برای تغییر ساختار داده ویرایش کنید.

### API Clients
فایل‌های `*ApiClient.cs` را برای تغییر منطق ارتباط با API ویرایش کنید.

## مزایای استفاده از DTO های موجود

### ✅ Type Safety
- استفاده از همان type های موجود در Application layer
- جلوگیری از type mismatch
- IntelliSense support بهتر

### ✅ Consistency
- یکپارچگی در ساختار داده
- تغییرات در یک جا اعمال می‌شود
- کاهش duplication

### ✅ Maintainability
- نگهداری آسان‌تر
- تغییرات خودکار در همه جا اعمال می‌شود
- کاهش احتمال خطا

## مزایای استفاده از Typed API Clients

### ✅ Dependency Inversion
- Controller دیگر route API را نمی‌داند
- وابستگی به abstraction نه implementation
- تست‌پذیری بهتر

### ✅ Separation of Concerns
- جداسازی منطق API communication از Controller
- مسئولیت‌های مشخص و جداگانه
- کد تمیزتر و قابل نگهداری‌تر

### ✅ Reusability
- امکان استفاده مجدد از API client در جاهای دیگر
- کاهش duplication در کد
- مدیریت متمرکز API calls

### ✅ Error Handling
- مدیریت خطا در یک مکان
- Logging متمرکز
- Error messages یکپارچه

### ✅ Extensibility
- اضافه کردن آسان endpoint های جدید
- تغییر آسان implementation
- پشتیبانی از caching و retry logic

## Troubleshooting

### Common Issues
1. **Database Connection**: اطمینان از اجرای migrations
2. **CORS Issues**: بررسی تنظیمات CORS در API
3. **Session Issues**: بررسی تنظیمات session در MVC
4. **External Service**: بررسی URL و تنظیمات ExternalService

### Debug Tips
- بررسی logs در `Backend.Api/logs/`
- استفاده از browser developer tools
- بررسی network requests
- بررسی session data

## Future Enhancements

### Phase 2 Features
- [ ] Email verification
- [ ] Password reset
- [ ] Two-factor authentication
- [ ] Social login (Google, Facebook)
- [ ] Profile management
- [ ] Account settings

### Phase 3 Features
- [ ] Role-based authorization
- [ ] Permission system
- [ ] Audit logging
- [ ] Advanced security features 