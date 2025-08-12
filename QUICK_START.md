# Quick Start Guide

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### 1. Database Setup

```bash
# Update connection strings in appsettings.json
# Run migrations for both databases
dotnet ef database update --project Backend.Identity
dotnet ef database update --project Backend.Persistence
```

### 2. Configuration

Update `appsettings.json` in both API and MVC projects:

```json
{
  "ConnectionStrings": {
    "DBConnection": "Data Source=.;Initial catalog=Backend_DB;Integrated security=true;TrustServerCertificate=True;",
    "IdentityDBConnection": "Data Source=.;Initial catalog=BackendIdentity_DB;Integrated security=true;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-256-bits",
    "Issuer": "Backend.Api",
    "Audience": "Backend.Client",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

### 3. Run the Applications

```bash
# Terminal 1 - Run API
cd Backend.Api
dotnet run

# Terminal 2 - Run MVC Client
cd Client.MVC
dotnet run
```

### 4. Access the Applications

- **API**: https://localhost:7209
- **MVC Client**: https://localhost:7000
- **Swagger**: https://localhost:7209/swagger

## 🔧 Key Features

### Security Improvements
- ✅ Strongly-typed DTOs (no more `dynamic`)
- ✅ JWT with Refresh Tokens
- ✅ CSRF Protection
- ✅ Rate Limiting
- ✅ Global Error Handling
- ✅ Secure Session Storage

### Architecture
- ✅ Clean Architecture
- ✅ CQRS with MediatR
- ✅ Domain-Driven Design
- ✅ Repository Pattern
- ✅ Unit of Work

## 📚 Documentation

- [Security Improvements](SECURITY_IMPROVEMENTS.md)
- [Usage Guide](USAGE_GUIDE.md)
- [API Documentation](https://localhost:7209/swagger)

## 🛠️ Troubleshooting

### Common Issues

1. **Session Error**: Add `AddDistributedMemoryCache()` before `AddSession()`
2. **Database Connection**: Check connection strings and SQL Server
3. **JWT Issues**: Verify JWT settings in appsettings.json
4. **CORS Issues**: Check API and client URLs match

### Development Tips

- Use `ExternalServiceSimple` instead of `ExternalService` (Polly compatibility)
- Let middleware handle errors (no try-catch in controllers)
- Use `TokenService` for token management
- Always use strongly-typed DTOs

## 🎯 Next Steps

1. Add your business logic
2. Implement additional endpoints
3. Add unit tests
4. Configure production settings
5. Add logging and monitoring

## 📞 Support

For issues and questions:
- Check the troubleshooting section
- Review the documentation
- Check the security improvements guide 