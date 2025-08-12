# Security Improvements Usage Guide

## 🚀 Quick Start

### 1. Authentication Flow

```csharp
// In your MVC controller
public class AccountController : Controller
{
    private readonly IExternalService _externalService;
    private readonly ITokenService _tokenService;

    public AccountController(IExternalService externalService, ITokenService tokenService)
    {
        _externalService = externalService;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return View(loginDto);

        try
        {
            // Use strongly-typed DTOs instead of dynamic
            var response = await _externalService.PostAsync<LoginDto, AuthResponseDto>("api/auth/login", loginDto);

            if (response.IsSuccess && !string.IsNullOrEmpty(response.Token))
            {
                // Store tokens securely using TokenService
                _tokenService.StoreTokens(
                    response.Token,
                    response.RefreshToken ?? string.Empty,
                    response.UserName ?? string.Empty,
                    response.UserId ?? string.Empty,
                    response.ExpiresAt ?? DateTime.UtcNow.AddHours(1)
                );

                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", response.Message ?? "Login failed");
                return View(loginDto);
            }
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(loginDto);
        }
    }
}
```

### 2. Token Management

```csharp
// Check if user has valid token
if (_tokenService.HasValidToken())
{
    // User is authenticated
    var userName = _tokenService.GetUserName();
    var userId = _tokenService.GetUserId();
}

// Check if token is expiring soon
if (_tokenService.IsTokenExpiringSoon())
{
    // Refresh token
    var refreshRequest = new RefreshTokenRequestDto 
    { 
        RefreshToken = _tokenService.GetRefreshToken() 
    };
    
    var refreshResponse = await _externalService.PostAsync<RefreshTokenRequestDto, RefreshTokenResponseDto>(
        "api/auth/refresh-token", 
        refreshRequest
    );
    
    if (refreshResponse.IsSuccess)
    {
        _tokenService.StoreTokens(
            refreshResponse.Token,
            refreshResponse.RefreshToken,
            _tokenService.GetUserName() ?? string.Empty,
            _tokenService.GetUserId() ?? string.Empty,
            refreshResponse.ExpiresAt ?? DateTime.UtcNow.AddHours(1)
        );
    }
}

// Clear tokens on logout
_tokenService.ClearTokens();
```

### 3. API Controller (No Try-Catch Needed)

```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public CustomerController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        // No try-catch needed - ErrorHandlingMiddleware handles all exceptions
        var result = await _commandDispatcher.DispatchAsync(command);
        
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] GetAllCustomersQuery query)
    {
        // No try-catch needed - ErrorHandlingMiddleware handles all exceptions
        var result = await _queryDispatcher.DispatchAsync(query);
        
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }
}
```

### 4. External Service Usage

```csharp
// GET request
var customers = await _externalService.GetAsync<List<CustomerDto>>("api/customers", new { page = 1, size = 10 });

// POST request with strongly-typed DTOs
var createCustomerCommand = new CreateCustomerCommand 
{ 
    FirstName = "John", 
    LastName = "Doe", 
    Email = "john@example.com" 
};

var result = await _externalService.PostAsync<CreateCustomerCommand, CustomerDto>("api/customers", createCustomerCommand);

// PUT request
var updateCommand = new UpdateCustomerCommand { Id = 1, FirstName = "Jane" };
var updatedCustomer = await _externalService.PutAsync<UpdateCustomerCommand, CustomerDto>("api/customers/1", updateCommand);

// DELETE request
await _externalService.DeleteAsync("api/customers/1");
```

## 🔧 Configuration

### 1. Program.cs Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register services
builder.Services.AddScoped<IExternalService, ExternalServiceSimple>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure middleware pipeline
app.UseSession();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRateLimiting();

app.UseAuthentication();
app.UseAuthorization();
```

### 2. appsettings.json

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
  },
  "ExternalService": {
    "BaseUrl": "https://localhost:7209/",
    "TimeoutSeconds": 30,
    "UseHttps": true,
    "DefaultHeaders": {
      "Accept": "application/json"
    }
  }
}
```

## 🛡️ Security Features

### 1. Rate Limiting
- **Automatic**: 100 requests per minute, 1000 per hour
- **Per Client**: Based on IP address or user ID
- **Response**: 429 Too Many Requests with Retry-After header

### 2. CSRF Protection
- **Automatic**: All non-GET requests validated
- **Token Generation**: `GET /api/csrf/token`
- **Header**: Include `X-CSRF-TOKEN` in requests

### 3. Error Handling
- **Global**: All exceptions handled by middleware
- **Consistent**: Standardized error response format
- **Secure**: No sensitive data in production

### 4. Token Security
- **Session Storage**: Secure server-side storage
- **Automatic Expiration**: Built-in expiration checking
- **Refresh Flow**: Automatic token refresh

## 📋 Best Practices

### 1. Always Use Strongly-Typed DTOs
```csharp
// ✅ Good
var response = await _externalService.PostAsync<LoginDto, AuthResponseDto>("api/auth/login", loginDto);

// ❌ Bad
var response = await _externalService.PostAsync<LoginDto, dynamic>("api/auth/login", loginDto);
```

### 2. Use TokenService for Token Management
```csharp
// ✅ Good
_tokenService.StoreTokens(token, refreshToken, userName, userId, expiresAt);

// ❌ Bad
HttpContext.Session.Set("JWTToken", tokenBytes);
```

### 3. Let Middleware Handle Errors
```csharp
// ✅ Good
public async Task<IActionResult> SomeAction()
{
    var result = await _someService.DoSomething();
    return Ok(result);
}

// ❌ Bad
public async Task<IActionResult> SomeAction()
{
    try
    {
        var result = await _someService.DoSomething();
        return Ok(result);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
```

### 4. Validate Input Models
```csharp
[HttpPost]
public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
        
    var result = await _commandDispatcher.DispatchAsync(command);
    return Ok(result);
}
```

## 🔍 Troubleshooting

### Common Issues

1. **Polly Compilation Errors**
   - Solution: Use `ExternalServiceSimple` instead of `ExternalService`
   - Reason: Polly version compatibility issues

2. **Session Not Working**
   - Ensure `app.UseSession()` is called before other middleware
   - Check session configuration in Program.cs

3. **Rate Limiting Too Strict**
   - Adjust limits in `RateLimitingMiddleware`
   - Consider different limits for different endpoints

4. **CSRF Token Issues**
   - Ensure session is working
   - Check token is included in request headers
   - Verify token is generated before making requests

### Debug Mode

```csharp
// Enable detailed error messages in development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
```

## 📚 Additional Resources

- [Security Improvements Documentation](SECURITY_IMPROVEMENTS.md)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [JWT Best Practices](https://auth0.com/blog/a-look-at-the-latest-draft-for-jwt-bcp/)
- [OWASP Security Guidelines](https://owasp.org/www-project-top-ten/) 