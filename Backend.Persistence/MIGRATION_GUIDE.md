# Migration Guide for Backend.Persistence

## مشکل رایج

اگر با خطای زیر مواجه شدید:
```
Unable to create a 'DbContext' of type 'ApplicationDbContext'. 
The exception 'Unable to resolve service for type 'Microsoft.EntityFrameworkCore.DbContextOptions`1[Backend.Persistence.Contexts.ApplicationDbContext]' while attempting to activate 'Backend.Persistence.Contexts.ApplicationDbContext'.'
```

## راه‌حل‌ها

### 1. استفاده از PowerShell Script (توصیه شده)

```powershell
# در پوشه Backend.Persistence
.\migrate.ps1
```

### 2. اجرای دستی Migration

```bash
# نصب EF Tools (اگر نصب نیست)
dotnet tool install --global dotnet-ef

# اضافه کردن migration
dotnet ef migrations add InitialCreate --startup-project ..\Backend.Api\Backend.Api.csproj

# اعمال migration به دیتابیس
dotnet ef database update --startup-project ..\Backend.Api\Backend.Api.csproj
```

### 3. تنظیم Connection String

اطمینان حاصل کنید که connection string در `appsettings.json` تنظیم شده است:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BackendDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 4. بررسی Dependencies

اطمینان حاصل کنید که تمام dependencies در `Backend.Persistence.csproj` وجود دارند:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.8" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.8" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.8" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.8" />
```

## نکات مهم

1. **Design-time Factory**: `ApplicationDbContextFactory` برای حل مشکل dependencies در زمان design-time ایجاد شده است.

2. **Connection String**: اطمینان حاصل کنید که SQL Server LocalDB نصب و در حال اجرا است.

3. **Startup Project**: همیشه از `Backend.Api` به عنوان startup project استفاده کنید.

4. **Database**: اگر دیتابیس وجود ندارد، EF آن را به طور خودکار ایجاد می‌کند.

## عیب‌یابی

### خطای Connection
- SQL Server LocalDB را بررسی کنید
- Connection string را بررسی کنید
- Firewall settings را بررسی کنید

### خطای Dependencies
- `dotnet restore` را اجرا کنید
- NuGet packages را بررسی کنید
- Project references را بررسی کنید

### خطای Migration
- دیتابیس را بررسی کنید
- Migration history را بررسی کنید
- در صورت نیاز، دیتابیس را drop و دوباره ایجاد کنید 