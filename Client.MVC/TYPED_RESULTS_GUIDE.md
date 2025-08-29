# راهنمای کامل Typed Results با ApiResponse<T>

## 🎯 **هدف**

استفاده از `ApiResponse<T>` به جای `null` برای consistency بهتر و error handling قوی‌تر:

1. **Consistency**: همه API responses از یک format استفاده می‌کنن
2. **Error Handling**: اطلاعات کامل error در دسترس
3. **Type Safety**: Compile-time type checking
4. **Monitoring**: Logging و debugging بهتر

## 🏗️ **معماری ApiResponse<T>**

### **1. Structure**
```csharp
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }
    public int? StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Factory Methods
    public static ApiResponse<T> Success(T data, int? statusCode = 200) { /* ... */ }
    public static ApiResponse<T> Error(string errorMessage, int? statusCode = 400) { /* ... */ }
    public static ApiResponse<T> Cancelled(string? errorMessage = null) { /* ... */ }
}
```

### **2. Factory Methods**
```csharp
// ✅ Success Response
return ApiResponse<AuthResultDto>.Success(result, 200);

// ✅ Error Response
return ApiResponse<AuthResultDto>.Error("Invalid credentials", 401);

// ✅ Cancelled Response
return ApiResponse<AuthResultDto>.Cancelled();
```

## 🔧 **Implementation Details**

### **1. AuthenticatedHttpClient**

#### **Before (Nullable Types)**
```csharp
// ❌ Old way - returns null on failure
public async Task<TResponse?> GetAsync<TResponse>(string endpoint) where TResponse : class
{
    try
    {
        var response = await httpClient.GetAsync(endpoint);
        if (response.IsSuccessStatusCode)
        {
            return await DeserializeAsync<TResponse>(response);
        }
        return null; // ❌ No error information
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error");
        return null; // ❌ No error information
    }
}
```

#### **After (ApiResponse<T>)**
```csharp
// ✅ New way - returns ApiResponse with full context
public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint) where TResponse : class
{
    try
    {
        var response = await httpClient.GetAsync(endpoint);
        if (response.IsSuccessStatusCode)
        {
            var data = await DeserializeAsync<TResponse>(response);
            return ApiResponse<TResponse>.Success(data, (int)response.StatusCode);
        }
        return ApiResponse<TResponse>.Error($"Request failed: {response.StatusCode}", (int)response.StatusCode);
    }
    catch (OperationCanceledException)
    {
        return ApiResponse<TResponse>.Cancelled();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error");
        return ApiResponse<TResponse>.Error($"Error: {ex.Message}", 500);
    }
}
```

### **2. AuthApiClient**

#### **Before (Nullable Types)**
```csharp
// ❌ Old way - complex null checking
public async Task<ApiResponse<AuthResultDto>> LoginAsync(LoginDto dto)
{
    var result = await _httpClient.PostAsync<LoginDto, AuthResultDto>("api/Auth/login", dto);
    
    if (result?.IsSuccess == true)
    {
        return ApiResponse<AuthResultDto>.Success(result);
    }
    else
    {
        return result.ToApiResponse(400); // ❌ Complex conversion
    }
}
```

#### **After (ApiResponse<T>)**
```csharp
// ✅ New way - clean and consistent
public async Task<ApiResponse<AuthResultDto>> LoginAsync(LoginDto dto)
{
    var response = await _httpClient.PostAsync<LoginDto, AuthResultDto>("api/Auth/login", dto);
    
    if (response.IsSuccess && response.Data?.IsSuccess == true)
    {
        return ApiResponse<AuthResultDto>.Success(response.Data);
    }
    else
    {
        var errorMessage = response.Data?.ErrorMessage ?? response.ErrorMessage ?? "Login failed";
        return ApiResponse<AuthResultDto>.Error(errorMessage, response.StatusCode ?? 400);
    }
}
```

### **3. UserApiClient**

#### **Before (Nullable Types)**
```csharp
// ❌ Old way - returns null on failure
public async Task<UserDto?> GetUserByIdAsync(string userId)
{
    var result = await _httpClient.GetAsync<UserDto>($"api/User/{userId}");
    return result; // ❌ Could be null
}
```

#### **After (ApiResponse<T>)**
```csharp
// ✅ New way - handles errors gracefully
public async Task<UserDto?> GetUserByIdAsync(string userId)
{
    var response = await _httpClient.GetAsync<UserDto>($"api/User/{userId}");
    
    if (response.IsSuccess && response.Data != null)
    {
        return response.Data;
    }
    else
    {
        _logger.LogWarning("Failed to retrieve user: {UserId}, Error: {Error}", 
            userId, response.ErrorMessage);
        return null;
    }
}
```

## 🚀 **Usage Patterns**

### **1. Success Handling**
```csharp
var response = await _httpClient.GetAsync<UserDto>("api/User/123");

if (response.IsSuccess && response.Data != null)
{
    var user = response.Data;
    // ✅ Use user data
    _logger.LogInformation("User retrieved: {UserName}", user.UserName);
}
else
{
    // ✅ Handle error with context
    _logger.LogWarning("Failed to get user: {Error}", response.ErrorMessage);
}
```

### **2. Error Handling**
```csharp
var response = await _httpClient.PostAsync<LoginDto, AuthResultDto>("api/Auth/login", loginDto);

if (!response.IsSuccess)
{
    // ✅ Rich error information
    var statusCode = response.StatusCode ?? 500;
    var errorMessage = response.ErrorMessage ?? "Unknown error";
    
    _logger.LogWarning("Login failed: {StatusCode} - {Error}", statusCode, errorMessage);
    
    // Handle specific error types
    if (statusCode == 401)
    {
        // Handle authentication error
    }
    else if (statusCode == 429)
    {
        // Handle rate limiting
    }
}
```

### **3. Cancellation Handling**
```csharp
try
{
    var response = await _httpClient.GetAsync<UserDto>("api/User/123", cancellationToken);
    
    if (response.IsSuccess)
    {
        // Handle success
    }
    else if (response.ErrorMessage?.Contains("cancelled") == true)
    {
        // Handle cancellation
        _logger.LogInformation("Request was cancelled");
    }
}
catch (OperationCanceledException)
{
    // Handle cancellation at exception level
}
```

## 📊 **Benefits**

### **1. Consistency**
```csharp
// ✅ All methods return the same type
Task<ApiResponse<AuthResultDto>> LoginAsync(...)
Task<ApiResponse<UserDto>> GetUserAsync(...)
Task<ApiResponse<bool>> DeleteUserAsync(...)
Task<ApiResponse<PaginatedResult<UserDto>>> GetUsersAsync(...)
```

### **2. Error Context**
```csharp
// ✅ Rich error information
var response = await _httpClient.GetAsync<UserDto>("api/User/123");

if (!response.IsSuccess)
{
    // Available information:
    // - response.ErrorMessage: Detailed error message
    // - response.StatusCode: HTTP status code
    // - response.Timestamp: When the error occurred
    // - response.IsSuccess: Boolean flag
}
```

### **3. Type Safety**
```csharp
// ✅ Compile-time checking
var response = await _httpClient.GetAsync<UserDto>("api/User/123");

// Type-safe access to data
if (response.Data != null)
{
    var userName = response.Data.UserName; // ✅ Type-safe
    var email = response.Data.Email;       // ✅ Type-safe
}
```

### **4. Monitoring & Observability**
```csharp
// ✅ Better logging
_logger.LogInformation("API Response: Success={Success}, StatusCode={StatusCode}, Error={Error}", 
    response.IsSuccess, response.StatusCode, response.ErrorMessage);

// ✅ Metrics collection
if (response.IsSuccess)
{
    _metrics.IncrementCounter("api_success");
}
else
{
    _metrics.IncrementCounter("api_error", new Dictionary<string, string>
    {
        ["status_code"] = response.StatusCode?.ToString() ?? "unknown"
    });
}
```

## 🔧 **Error Handling Strategies**

### **1. Graceful Degradation**
```csharp
public async Task<UserDto?> GetUserProfileAsync()
{
    var response = await _httpClient.GetAsync<UserProfileDto>("api/User/profile");
    
    if (response.IsSuccess && response.Data != null)
    {
        return response.Data;
    }
    
    // ✅ Graceful degradation - return null instead of throwing
    _logger.LogWarning("Failed to get user profile: {Error}", response.ErrorMessage);
    return null;
}
```

### **2. Retry Logic**
```csharp
public async Task<ApiResponse<AuthResultDto>> LoginWithRetryAsync(LoginDto dto, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        var response = await _httpClient.PostAsync<LoginDto, AuthResultDto>("api/Auth/login", dto);
        
        if (response.IsSuccess)
        {
            return response;
        }
        
        // ✅ Don't retry on client errors (4xx)
        if (response.StatusCode >= 400 && response.StatusCode < 500)
        {
            return response;
        }
        
        // ✅ Retry on server errors (5xx)
        if (i < maxRetries - 1)
        {
            await Task.Delay(1000 * (i + 1)); // Exponential backoff
        }
    }
    
    return ApiResponse<AuthResultDto>.Error("Max retries exceeded", 500);
}
```

### **3. Circuit Breaker Integration**
```csharp
// ✅ ApiResponse works well with resilience policies
var response = await _resiliencePolicy.ExecuteAsync(async () =>
{
    return await _httpClient.GetAsync<UserDto>("api/User/123");
});

if (response.IsSuccess)
{
    // Handle success
}
else
{
    // Handle failure (could be circuit breaker, timeout, etc.)
    _logger.LogWarning("Request failed: {Error}", response.ErrorMessage);
}
```

## 📈 **Performance Considerations**

### **1. Memory Allocation**
```csharp
// ✅ ApiResponse<T> is a struct-like class - minimal overhead
public class ApiResponse<T>
{
    // Only essential properties
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }
    public int? StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### **2. Serialization**
```csharp
// ✅ Efficient JSON serialization
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
};

var response = JsonSerializer.Deserialize<ApiResponse<UserDto>>(json, jsonOptions);
```

## ✅ **Best Practices**

### **1. Always Check IsSuccess**
```csharp
// ✅ Good practice
var response = await _httpClient.GetAsync<UserDto>("api/User/123");

if (response.IsSuccess && response.Data != null)
{
    // Use response.Data
}
else
{
    // Handle error
    _logger.LogWarning("Request failed: {Error}", response.ErrorMessage);
}
```

### **2. Use StatusCode for Specific Handling**
```csharp
// ✅ Handle specific status codes
if (!response.IsSuccess)
{
    switch (response.StatusCode)
    {
        case 401:
            // Handle unauthorized
            break;
        case 403:
            // Handle forbidden
            break;
        case 404:
            // Handle not found
            break;
        case 429:
            // Handle rate limiting
            break;
        default:
            // Handle other errors
            break;
    }
}
```

### **3. Log Error Context**
```csharp
// ✅ Comprehensive logging
_logger.LogWarning("API request failed: Endpoint={Endpoint}, StatusCode={StatusCode}, Error={Error}", 
    endpoint, response.StatusCode, response.ErrorMessage);
```

### **4. Use Factory Methods**
```csharp
// ✅ Use factory methods for consistency
return ApiResponse<UserDto>.Success(user, 200);
return ApiResponse<UserDto>.Error("User not found", 404);
return ApiResponse<UserDto>.Cancelled();
```

## 🔄 **Migration Guide**

### **1. Update Interface**
```csharp
// ❌ Old interface
public interface IAuthenticatedHttpClient
{
    Task<TResponse?> GetAsync<TResponse>(string endpoint) where TResponse : class;
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request) 
        where TRequest : class where TResponse : class;
}

// ✅ New interface
public interface IAuthenticatedHttpClient
{
    Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint) where TResponse : class;
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request) 
        where TRequest : class where TResponse : class;
}
```

### **2. Update Implementation**
```csharp
// ❌ Old implementation
public async Task<TResponse?> GetAsync<TResponse>(string endpoint) where TResponse : class
{
    // ... implementation
    return result; // or null
}

// ✅ New implementation
public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint) where TResponse : class
{
    // ... implementation
    return ApiResponse<TResponse>.Success(result, (int)response.StatusCode);
}
```

### **3. Update Consumers**
```csharp
// ❌ Old consumer
var user = await _httpClient.GetAsync<UserDto>("api/User/123");
if (user != null)
{
    // Use user
}

// ✅ New consumer
var response = await _httpClient.GetAsync<UserDto>("api/User/123");
if (response.IsSuccess && response.Data != null)
{
    var user = response.Data;
    // Use user
}
```

این سیستم تضمین می‌کند که همه API responses consistent و type-safe باشند و error handling بهتری ارائه دهند. 