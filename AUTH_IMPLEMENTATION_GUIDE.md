# Authentication Implementation Guide

## Overview
This document explains the complete authentication chain implementation for user registration in the Backend API.

## Architecture Chain
```
API → Mediator → Handler → IUserService → UserService Implementation
```

## Components

### 1. API Layer (Backend.Api)
**File:** `Controllers/AuthController.cs`

- **AuthController**: Handles HTTP requests for authentication
- **Register Action**: 
  - Accepts `RegisterCommand` from client
  - Extracts client IP and User-Agent for security
  - Sends command through MediatR
  - Returns `Result<AuthResultDto>` to client

### 2. Application Layer (Backend.Application)
**Files:**
- `Features/UserManagement/Commands/Register/RegisterCommand.cs`
- `Features/UserManagement/Commands/Register/RegisterCommandHandler.cs`
- `Features/UserManagement/Commands/Register/RegisterCommandValidator.cs`
- `Common/Interfaces/IUserService.cs`

- **RegisterCommand**: Implements `ICommand<Result<AuthResultDto>>` (inherits from `IRequest`)
- **RegisterCommandHandler**: Implements `IRequestHandler<RegisterCommand, Result<AuthResultDto>>`
  - Converts command to `RegisterDto`
  - Calls `IUserService.RegisterAsync()`
  - Returns `Result<AuthResultDto>`
- **RegisterCommandValidator**: FluentValidation rules for command validation
- **IUserService**: Interface defining user management operations

### 3. Identity Layer (Backend.Identity)
**File:** `Services/UserService.cs`

- **UserService**: Implements `IUserService`
- **RegisterAsync Method**:
  - Validates user doesn't exist
  - Creates new `ApplicationUser`
  - Assigns default "User" role
  - Sends email confirmation
  - Returns `Result<AuthResultDto>`

## Dependency Injection Setup

### Program.cs (API Layer)
```csharp
// Register MediatR for API and Application layers
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly);
});
```

### IdentityServicesRegistration.cs
```csharp
// Register UserService
services.AddScoped<IUserService, UserService>();
```

### ApplicationServicesRegistration.cs
```csharp
// Register FluentValidation
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

## API Endpoint

### Register User
```
POST /api/auth/register
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

### Response
```json
{
  "isSuccess": true,
  "user": {
    "id": "user-id",
    "email": "test@example.com",
    "userName": "testuser",
    "emailConfirmed": false,
    "isActive": true
  },
  "requiresEmailConfirmation": true,
  "errorMessage": "Registration successful. Please check your email to confirm your account."
}
```

## Flow Diagram

```
Client Request
    ↓
AuthController.Register()
    ↓
RegisterCommand (with IP/UserAgent)
    ↓
MediatR.Send(RegisterCommand)
    ↓
RegisterCommandValidator (FluentValidation)
    ↓
RegisterCommandHandler.Handle()
    ↓
IUserService.RegisterAsync()
    ↓
UserService.RegisterAsync()
    ↓
ApplicationUser.Create()
    ↓
UserManager.CreateAsync()
    ↓
Role Assignment
    ↓
Email Confirmation
    ↓
Result<AuthResultDto>
    ↓
HTTP Response to Client
```

## Key Features

1. **Security**: Captures IP address and User-Agent for audit trails
2. **Validation**: FluentValidation ensures data integrity
3. **Error Handling**: Comprehensive error handling with meaningful messages
4. **Email Confirmation**: Automatic email confirmation setup
5. **Role Assignment**: Default "User" role assignment
6. **Logging**: Structured logging throughout the process

## Testing

Use the provided HTTP test file: `Backend.Api/Backend.Api.http`

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

## Notes

- The implementation follows Clean Architecture principles
- MediatR handles the CQRS pattern implementation
- FluentValidation provides comprehensive input validation
- The UserService in Identity layer handles all user management operations
- Email confirmation is automatically triggered after successful registration 