# راهنمای امنیت Logging برای Authentication

## 🎯 **هدف**

این راهنما برای اطمینان از عدم لاگ شدن اطلاعات حساس در سیستم authentication طراحی شده:

1. **عدم لاگ شدن Passwords** - هیچ‌وقت password لاگ نشود
2. **عدم لاگ شدن Tokens** - هیچ‌وقت actual token value لاگ نشود
3. **Structured Logging** - استفاده از structured logging در production
4. **Log Level Management** - مدیریت مناسب log levels

## 🛡️ **قوانین امنیتی Logging**

### **1. اطلاعاتی که هرگز لاگ نشوند**
```csharp
// ❌ NEVER LOG THESE
- Passwords (plain text or hashed)
- JWT tokens (access tokens, refresh tokens)
- API keys
- Connection strings
- Private keys
- Credit card numbers
- Social security numbers
- Personal identification numbers
```

### **2. اطلاعاتی که می‌توانند لاگ شوند**
```csharp
// ✅ SAFE TO LOG
- User ID (GUID)
- Email address (for debugging)
- Username
- Operation status (success/failure)
- Error messages (without sensitive data)
- Timestamps
- Request paths (without query parameters)
- HTTP status codes
```

## 📝 **نمونه‌های صحیح Logging**

### **1. Authentication Operations**
```csharp
// ✅ GOOD - Login attempt
_logger.LogInformation("Attempting to login user: {Email}", dto.EmailOrUsername);

// ✅ GOOD - Login success
_logger.LogInformation("User login successful: {Email}", dto.EmailOrUsername);

// ✅ GOOD - Login failure
_logger.LogWarning("User login failed: {Email}. Error: {Error}", 
    dto.EmailOrUsername, result?.ErrorMessage);

// ❌ BAD - Never log passwords
_logger.LogInformation("Login attempt with password: {Password}", dto.Password);
```

### **2. Token Operations**
```csharp
// ✅ GOOD - Token refresh attempt
_logger.LogInformation("Attempting to refresh token");

// ✅ GOOD - Token refresh success
_logger.LogInformation("Token refresh successful");

// ✅ GOOD - Token validation
_logger.LogDebug("JWT token stored in HttpOnly cookie with SameSite: {SameSite}", 
    _cookieConfig.JwtTokenSameSite);

// ❌ BAD - Never log actual tokens
_logger.LogInformation("Token value: {Token}", result.AccessToken);
```

### **3. Error Handling**
```csharp
// ✅ GOOD - Exception with context
_logger.LogError(ex, "Error during user login: {Email}", dto.EmailOrUsername);

// ✅ GOOD - Operation cancellation
_logger.LogInformation("User login cancelled for: {Email}", dto.EmailOrUsername);

// ❌ BAD - Never log sensitive data in errors
_logger.LogError(ex, "Login failed with password: {Password}", dto.Password);
```

## 🔧 **Structured Logging در Production**

### **1. Log Format**
```csharp
// ✅ Structured logging with properties
_logger.LogInformation("User login successful: {Email} {UserId} {Timestamp}", 
    dto.EmailOrUsername, result.User.Id, DateTime.UtcNow);

// ❌ String concatenation
_logger.LogInformation("User login successful: " + dto.EmailOrUsername);
```

### **2. Log Levels**
```csharp
// ✅ Appropriate log levels
_logger.LogDebug("Token validation details");      // Development only
_logger.LogInformation("User login successful");   // Production
_logger.LogWarning("Login failed");               // Production
_logger.LogError("System error");                 // Production
```

## 📊 **Monitoring & Observability**

### **1. Context Information**
```csharp
// ✅ Rich context for debugging
_logger.LogInformation(
    "Auth retry {RetryAttempt} after {Delay}ms for {OperationKey}. " +
    "Status: {StatusCode}, Exception: {Exception}",
    retryAttempt, timespan.TotalMilliseconds, context.OperationKey,
    outcome.Result?.StatusCode, outcome.Exception?.Message);
```

### **2. Correlation IDs**
```csharp
// ✅ Include correlation ID for tracing
_logger.LogInformation("Request {CorrelationId} processed for user: {Email}", 
    correlationId, userEmail);
```

## 🚀 **Best Practices**

### **1. Log Level Strategy**
```csharp
// Development
"LogLevel": {
  "Default": "Debug",
  "Microsoft.AspNetCore": "Information"
}

// Production
"LogLevel": {
  "Default": "Information",
  "Microsoft.AspNetCore": "Warning",
  "Client.MVC.Services": "Information"
}
```

### **2. Sensitive Data Filtering**
```csharp
// ✅ Use constants for sensitive field names
public static class SensitiveFields
{
    public const string Password = "password";
    public const string Token = "token";
    public const string ApiKey = "apiKey";
}

// ✅ Filter sensitive data in logging
private string SanitizeLogMessage(string message)
{
    return message.Replace("password=", "password=***")
                 .Replace("token=", "token=***");
}
```

### **3. Environment-Specific Logging**
```csharp
// Development - More verbose
if (_environment.IsDevelopment())
{
    _logger.LogDebug("Detailed debug information: {Details}", debugInfo);
}

// Production - Minimal sensitive data
if (_environment.IsProduction())
{
    _logger.LogInformation("Operation completed for user: {UserId}", userId);
}
```

## 🔍 **Security Auditing**

### **1. Log Review Checklist**
- [ ] No passwords in logs
- [ ] No tokens in logs
- [ ] No API keys in logs
- [ ] No connection strings in logs
- [ ] Structured logging used
- [ ] Appropriate log levels
- [ ] Context information included

### **2. Automated Scanning**
```csharp
// ✅ Use tools to scan for sensitive data
// - SonarQube
// - CodeQL
// - Custom regex patterns
```

## 📋 **Code Review Guidelines**

### **1. Pre-commit Checks**
```bash
# Scan for sensitive data patterns
grep -r "password\|token\|secret" --include="*.cs" . | grep -i log
```

### **2. Review Questions**
- Does this log contain sensitive data?
- Is the log level appropriate?
- Is structured logging used?
- Is context information included?
- Is this log necessary for debugging/monitoring?

## 🛠️ **Tools & Utilities**

### **1. Logging Extensions**
```csharp
public static class SecureLoggingExtensions
{
    public static void LogUserOperation(this ILogger logger, string operation, string email, bool success)
    {
        logger.LogInformation("User {Operation} {Status}: {Email}", 
            operation, success ? "successful" : "failed", email);
    }
}
```

### **2. Sensitive Data Sanitizer**
```csharp
public static class LogSanitizer
{
    public static string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        return input
            .Replace("password=", "password=***")
            .Replace("token=", "token=***")
            .Replace("secret=", "secret=***");
    }
}
```

## ✅ **Validation Checklist**

### **1. Authentication Logging**
- [ ] Login attempts logged (email only)
- [ ] Login success logged (email only)
- [ ] Login failures logged (email + error message)
- [ ] No passwords in logs
- [ ] No tokens in logs

### **2. Token Management**
- [ ] Token refresh attempts logged
- [ ] Token refresh success logged
- [ ] Token validation logged (without actual token)
- [ ] Token expiration logged (timestamp only)
- [ ] No actual token values in logs

### **3. Error Handling**
- [ ] Exceptions logged with context
- [ ] No sensitive data in error messages
- [ ] Appropriate log levels used
- [ ] Structured logging format

### **4. Production Readiness**
- [ ] Log levels configured for production
- [ ] Sensitive data filtered
- [ ] Structured logging enabled
- [ ] Monitoring-friendly format
- [ ] Performance impact considered

## 🔧 **Troubleshooting**

### **1. Common Issues**
- Sensitive data accidentally logged
- Inappropriate log levels
- Missing context information
- Performance impact from excessive logging

### **2. Debugging Without Sensitive Data**
```csharp
// ✅ Good debugging approach
_logger.LogDebug("Processing request for user: {UserId} with operation: {Operation}", 
    userId, operation);

// ❌ Bad debugging approach
_logger.LogDebug("Processing request with data: {RequestData}", 
    JsonSerializer.Serialize(requestData)); // Might contain sensitive data
```

## 📈 **Monitoring & Alerting**

### **1. Security Alerts**
- Unusual login patterns
- Failed authentication attempts
- Token refresh failures
- System errors

### **2. Performance Monitoring**
- Log volume
- Log processing time
- Storage usage
- Query performance

این راهنما تضمین می‌کند که سیستم authentication شما از نظر امنیت logging در سطح enterprise باشد. 