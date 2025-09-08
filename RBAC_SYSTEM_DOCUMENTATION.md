# Role-Based Access Control (RBAC) System

## Overview
This document describes the implementation of a comprehensive Role-Based Access Control (RBAC) system that ensures proper security and access management across the application.

## 🎯 **Core Principles**

### 1️⃣ **Default User Role Assignment**
- **All new users** automatically receive the `User` role during registration
- **No automatic Admin/SuperAdmin assignment** - these roles must be manually assigned
- **Secure by default** - new users have minimal permissions

### 2️⃣ **Role Hierarchy & Restrictions**
- **User**: Basic access to public content and personal profile
- **Admin**: Can manage regular users and content, but **cannot**:
  - Assign Admin or SuperAdmin roles to anyone
  - Modify system settings
  - Access audit logs
  - Manage security settings
- **SuperAdmin**: Full system access with no restrictions

### 3️⃣ **Authorization Policies**
The system uses policy-based authorization for fine-grained access control:

```csharp
// Basic role policies
RequireUser          // User, Admin, SuperAdmin
RequireAdmin         // Admin, SuperAdmin  
RequireSuperAdmin    // SuperAdmin only

// Management policies
CanManageUsers       // Admin, SuperAdmin
CanManageRoles       // SuperAdmin only
CanManagePermissions // SuperAdmin only
CanManageAdminRoles  // SuperAdmin only

// System policies
CanManageSystem      // SuperAdmin only
CanAccessAuditLogs   // SuperAdmin only
CanManageSecurity    // SuperAdmin only
```

## 🔧 **Implementation Details**

### Clean Architecture Compliance
The RBAC system follows Clean Architecture principles:

- **Application Layer**: Contains only business logic, policies, and requirements
- **Infrastructure Layer**: Handles DI container configuration and service registration
- **No cross-layer dependencies**: Application layer doesn't reference Infrastructure

### Registration Process
```csharp
// In ApplicationUserService.RegisterAsync()
var result = await _userManager.CreateAsync(user, registerDto.Password);
if (result.Succeeded)
{
    // Automatically assign "User" role to new users
    await _userManager.AddToRoleAsync(user, "User");
}
```

### Service Registration (Infrastructure Layer)
```csharp
// In InfrastructureServicesRegistration.cs
services.AddRoleBasedAuthorization(); // Registers all RBAC policies and handlers
```

### Controller Authorization
```csharp
[Authorize(Policy = RoleBasedAuthorizationPolicies.CanManageUsers)]
public class UsersController : AdminBaseController
{
    // Admin and SuperAdmin can access
}

[Authorize(Policy = RoleBasedAuthorizationPolicies.CanManageRoles)]
public class RolesController : AdminBaseController
{
    // Only SuperAdmin can access
}
```

### Middleware Protection
The `AdminAreaAccessMiddleware` ensures:
- Unauthenticated users → Login page
- Regular users → Access Denied
- Admin/SuperAdmin users → Admin area access

## 📊 **Role Matrix**

| Action | User | Admin | SuperAdmin |
|--------|------|-------|------------|
| View public content | ✅ | ✅ | ✅ |
| Edit own profile | ✅ | ✅ | ✅ |
| Access Admin area | ❌ | ✅ | ✅ |
| Manage users | ❌ | ✅ | ✅ |
| Create/Edit roles | ❌ | ❌ | ✅ |
| Assign Admin roles | ❌ | ❌ | ✅ |
| System settings | ❌ | ❌ | ✅ |
| Audit logs | ❌ | ❌ | ✅ |

## 🛡️ **Security Features**

### 1. **Automatic Role Assignment**
- New users get `User` role by default
- No escalation without explicit assignment
- Logged for audit purposes

### 2. **Policy-Based Authorization**
- Fine-grained permissions
- Centralized policy management
- Easy to extend and modify

### 3. **Controller-Level Protection**
- Each admin controller uses appropriate policies
- Prevents unauthorized access at the controller level
- Clear separation of concerns

### 4. **Middleware Security**
- Early interception of unauthorized requests
- Proper redirect handling
- Audit logging of access attempts

## 🚀 **Usage Examples**

### Creating a New User (Admin)
```csharp
// Admin can create users, but they get "User" role by default
var newUser = await _userService.CreateUserAsync(createUserDto, "admin@example.com");
// newUser will have "User" role, not "Admin"
```

### Assigning Admin Role (SuperAdmin Only)
```csharp
// Only SuperAdmin can promote users to Admin
if (currentUser.IsSuperAdmin)
{
    await _userService.AssignRolesAsync(userId, new[] { "Admin" }, "superadmin@example.com");
}
```

### Checking Permissions
```csharp
// Check if user can manage roles
if (User.IsInRole("SuperAdmin"))
{
    // Show role management UI
}
else
{
    // Hide role management options
}
```

## 📝 **Files Modified**

### Backend Changes
- `Backend.Identity/Services/ApplicationUserService.cs` - Added default role assignment
- `Backend.Application/Common/Authorization/RoleBasedAuthorizationPolicies.cs` - Business logic and policies
- `Backend.Infrastructure/Authorization/RoleBasedAuthorizationServiceCollectionExtensions.cs` - DI configuration
- `Backend.Infrastructure/DependencyInjection/InfrastructureServicesRegistration.cs` - Service registration

### Frontend Changes
- `Client.MVC/Areas/Admin/Controllers/RolesController.cs` - Updated authorization
- `Client.MVC/Areas/Admin/Controllers/UsersController.cs` - Updated authorization
- `Client.MVC/Middleware/AdminAreaAccessMiddleware.cs` - Enhanced security

## 🔍 **Testing Scenarios**

### Scenario 1: New User Registration
1. User registers → Gets `User` role automatically
2. Tries to access `/Admin` → Redirected to Access Denied
3. Can only access public content and own profile

### Scenario 2: Admin User Management
1. Admin logs in → Can access Admin area
2. Can manage regular users → Can assign/remove `User` role
3. Cannot assign `Admin` or `SuperAdmin` roles → UI hides these options

### Scenario 3: SuperAdmin Full Access
1. SuperAdmin logs in → Full system access
2. Can manage all users and roles
3. Can access system settings and audit logs

## 🎉 **Benefits**

✅ **Security**: No unauthorized role escalation  
✅ **Scalability**: Easy to add new roles and permissions  
✅ **Maintainability**: Centralized policy management  
✅ **Auditability**: Complete access logging  
✅ **Flexibility**: Fine-grained permission control  

## 🔮 **Future Enhancements**

- **Permission-based authorization**: More granular than role-based
- **Dynamic role assignment**: Based on user attributes
- **Role inheritance**: Hierarchical role structures
- **Time-based permissions**: Temporary access grants
- **Multi-tenant support**: Organization-specific roles