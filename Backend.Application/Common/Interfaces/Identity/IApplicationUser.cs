using System;
using System.Collections.Generic;

namespace Backend.Application.Common.Interfaces.Identity;

/// <summary>
/// Interface for ApplicationUser to avoid dependency on Identity layer
/// </summary>
public interface IApplicationUser
{
    // Core Identity Properties
    string Id { get; }
    string Email { get; }
    string UserName { get; }
    string? PhoneNumber { get; }
    bool EmailConfirmed { get; }
    bool PhoneNumberConfirmed { get; }
    bool TwoFactorEnabled { get; }
    bool LockoutEnabled { get; }
    int AccessFailedCount { get; }
    DateTimeOffset? LockoutEnd { get; }
    
    // Custom Properties
    Guid? CustomerId { get; }
    string? TotpSecretKey { get; }
    bool TotpEnabled { get; }
    bool SmsEnabled { get; }
    string? GoogleId { get; }
    string? MicrosoftId { get; }
    
    // Account Info
    IAccountInfo Account { get; }
    
    // Security Info
    ISecurityInfo Security { get; }
    
    // Audit Info
    IAuditInfo Audit { get; }
    
    // Navigation Properties
    List<string> Roles { get; }
    
    // Computed Properties
    bool IsLocked { get; }
    bool IsAccountLocked { get; }
    bool IsActive { get; }
    bool IsDeleted { get; }
    bool IsNewUser { get; }
    
    // Additional Properties for DTO mapping
    DateTime? LastLoginAt { get; }
    DateTime? LastPasswordChangeAt { get; }
    int LoginAttempts { get; }
    bool RequiresPasswordChange { get; }
    
    // Business Logic Methods
    bool CanLogin();
}

/// <summary>
/// Interface for AccountInfo value object
/// </summary>
public interface IAccountInfo
{
    bool IsActive { get; }
    bool IsDeleted { get; }
    bool IsLocked();
    DateTime CreatedAt { get; }
    DateTime? LastPasswordChangeAt { get; }
}

/// <summary>
/// Interface for SecurityInfo value object
/// </summary>
public interface ISecurityInfo
{
    string? SecurityQuestion { get; }
    string? SecurityAnswer { get; }
    DateTime? LastSecurityUpdate { get; }
}

/// <summary>
/// Interface for AuditInfo value object
/// </summary>
public interface IAuditInfo
{
    string? CreatedBy { get; }
    DateTime CreatedAt { get; }
    string? LastModifiedBy { get; }
    DateTime? LastModifiedAt { get; }
}