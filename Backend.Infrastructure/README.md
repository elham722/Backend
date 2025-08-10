# Backend.Infrastructure

## Overview

The Infrastructure layer contains all external concerns and implementations of interfaces defined in the Application layer. This layer handles:

- External services integration
- Email services
- Caching services
- File storage services
- Current user services
- DateTime services
- Local storage services

## Architecture

```
Backend.Infrastructure/
├── Cache/                    # Caching implementations
├── Email/                    # Email service implementations
├── ExternalServices/         # External API integrations
├── Extensions/               # Infrastructure extensions
├── FileStorage/              # File storage implementations
├── LocalStorage/             # Local storage implementations
├── Services/                 # Core infrastructure services
├── EmailTemplates/           # Email templates
└── DependencyInjection/      # Service registration
```

## Services

### 1. Email Services

#### IEmailSender Interface
- **SendGridEmailSender**: SendGrid email service implementation
- **SmtpEmailSender**: SMTP email service implementation
- **EmailTemplateService**: Email template management

#### Configuration
```json
{
  "EmailSettings": {
    "Provider": "SendGrid",
    "ApiKey": "your-sendgrid-api-key",
    "FromAddress": "noreply@yourapp.com",
    "FromName": "Your App"
  }
}
```

### 2. Cache Services

#### ICacheService Interface
- **MemoryCacheService**: In-memory caching implementation

#### Features
- Get/Set operations
- Async operations
- Expiration management
- GetOrSet pattern
- Existence checking

#### Configuration
```json
{
  "MemoryCache": {
    "DefaultExpirationMinutes": 60,
    "SizeLimit": 1000
  }
}
```

### 3. File Storage Services

#### IFileStorageService Interface
- **LocalFileStorageService**: Local file system storage

#### Features
- File upload/download
- File deletion
- File existence checking
- File information retrieval
- Temporary URL generation
- File listing

#### Configuration
```json
{
  "LocalFileStorage": {
    "BasePath": "wwwroot/uploads",
    "MaxFileSizeBytes": 10485760,
    "AllowedExtensions": [".jpg", ".png", ".pdf"]
  }
}
```

### 4. External Services

#### IExternalService Interface
- **ExternalService**: HTTP client wrapper for external APIs

#### Features
- GET, POST, PUT, DELETE operations
- Query parameter support
- Bearer token authentication
- Error handling
- JSON serialization

#### Configuration
```json
{
  "ExternalService": {
    "BaseUrl": "https://api.external.com",
    "TimeoutSeconds": 30,
    "DefaultHeaders": {
      "User-Agent": "YourApp/1.0"
    }
  }
}
```

### 5. Core Services

#### ICurrentUserService
- **CurrentUserService**: HttpContext-based user information

#### Features
- User ID, username, email extraction
- Role and permission checking
- Authentication status
- Claim value retrieval

#### IDateTimeService
- **DateTimeService**: DateTime utilities

#### Features
- Current date/time (local and UTC)
- Timezone conversion
- Business day calculations
- Date manipulation utilities

### 6. Local Storage

#### ILocalStorageService
- **LocalStorageService**: Local storage implementation

#### Features
- Key-value storage
- Type-safe operations
- Existence checking
- Value retrieval

## Dependency Injection

### Registration

```csharp
// In Program.cs or Startup.cs
services.AddInfrastructureServices(configuration);
```

### Custom Configuration

```csharp
services.AddInfrastructureServices(options =>
{
    options.EmailProvider = "SendGrid";
    options.EmailApiKey = "your-api-key";
    options.FileStorageBasePath = "uploads";
    options.CacheExpirationMinutes = 30;
});
```

## Usage Examples

### Email Service

```csharp
public class EmailController : ControllerBase
{
    private readonly IEmailSender _emailSender;

    public EmailController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task<IActionResult> SendWelcomeEmail(string email)
    {
        var success = await _emailSender.SendEmailAsync(
            email,
            "Welcome to Our App",
            "<h1>Welcome!</h1><p>Thank you for joining us.</p>",
            isHtml: true);

        return Ok(new { Success = success });
    }
}
```

### Cache Service

```csharp
public class CustomerService
{
    private readonly ICacheService _cacheService;

    public CustomerService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<CustomerDto> GetCustomerAsync(Guid id)
    {
        var cacheKey = $"customer:{id}";
        
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            // Fetch from database
            return await _customerRepository.GetByIdAsync(id);
        }, expirationMinutes: 30);
    }
}
```

### File Storage Service

```csharp
public class FileController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;

    public FileController(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var filePath = await _fileStorage.UploadFileAsync(
            stream,
            file.FileName,
            file.ContentType,
            folder: "documents");

        return Ok(new { FilePath = filePath });
    }
}
```

### External Service

```csharp
public class ExternalApiService
{
    private readonly IExternalService _externalService;

    public ExternalApiService(IExternalService externalService)
    {
        _externalService = externalService;
    }

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        return await _externalService.GetAsync<WeatherData>(
            $"/weather/{city}");
    }
}
```

## Configuration

### appsettings.json

```json
{
  "EmailSettings": {
    "Provider": "SendGrid",
    "ApiKey": "your-sendgrid-api-key",
    "FromAddress": "noreply@yourapp.com",
    "FromName": "Your App"
  },
  "MemoryCache": {
    "DefaultExpirationMinutes": 60,
    "SizeLimit": 1000
  },
  "LocalFileStorage": {
    "BasePath": "wwwroot/uploads",
    "MaxFileSizeBytes": 10485760,
    "AllowedExtensions": [".jpg", ".png", ".pdf", ".doc", ".docx"]
  },
  "ExternalService": {
    "BaseUrl": "https://api.external.com",
    "TimeoutSeconds": 30,
    "DefaultHeaders": {
      "User-Agent": "YourApp/1.0"
    }
  },
  "LocalStorage": {
    "BasePath": "local-storage"
  }
}
```

## Best Practices

### 1. Service Registration
- Always register services in the correct order
- Use scoped services for request-scoped data
- Configure options through configuration files

### 2. Error Handling
- Implement proper exception handling
- Log errors appropriately
- Return meaningful error messages

### 3. Security
- Never hardcode sensitive information
- Use configuration for API keys
- Validate file uploads
- Sanitize file names

### 4. Performance
- Use caching appropriately
- Implement async operations
- Monitor service performance
- Use connection pooling

### 5. Testing
- Mock external services in unit tests
- Use in-memory providers for testing
- Test error scenarios
- Validate configuration

## Dependencies

- **Backend.Application**: Application layer interfaces
- **Microsoft.Extensions.Caching.Memory**: Memory caching
- **Microsoft.Extensions.Http**: HTTP client factory
- **SendGrid**: Email service
- **Microsoft.AspNetCore.Http.Abstractions**: HTTP context access

## Notes

- All services implement interfaces from the Application layer
- Services are designed to be easily testable and mockable
- Configuration is externalized for flexibility
- Error handling is consistent across all services
- Async operations are used where appropriate 