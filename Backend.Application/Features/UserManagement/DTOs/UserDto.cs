using Backend.Application.Common.DTOs;
using System;

namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for user information
/// </summary>
public class UserDto : BaseDto
{
    /// <summary>
    /// User's unique identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's username
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Whether email is confirmed
    /// </summary>
    public bool EmailConfirmed { get; set; }
    
    /// <summary>
    /// Whether phone number is confirmed
    /// </summary>
    public bool PhoneNumberConfirmed { get; set; }
    
    /// <summary>
    /// Whether two-factor authentication is enabled
    /// </summary>
    public bool TwoFactorEnabled { get; set; }
    
    /// <summary>
    /// Whether lockout is enabled
    /// </summary>
    public bool LockoutEnabled { get; set; }
    
    /// <summary>
    /// Number of failed access attempts
    /// </summary>
    public int AccessFailedCount { get; set; }
    
    /// <summary>
    /// Whether account is locked
    /// </summary>
    public bool IsLocked { get; set; }
    
    /// <summary>
    /// Whether account is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Whether account is deleted
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Last login date
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Last password change date
    /// </summary>
    public DateTime? LastPasswordChangeAt { get; set; }
    
    /// <summary>
    /// Number of failed login attempts
    /// </summary>
    public int LoginAttempts { get; set; }
    
    /// <summary>
    /// User roles
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// Associated customer ID (if any)
    /// </summary>
    public string? CustomerId { get; set; }
    
    /// <summary>
    /// TOTP secret key (if enabled)
    /// </summary>
    public string? TotpSecretKey { get; set; }
    
    /// <summary>
    /// Whether TOTP is enabled
    /// </summary>
    public bool TotpEnabled { get; set; }
    
    /// <summary>
    /// Whether SMS MFA is enabled
    /// </summary>
    public bool SmsEnabled { get; set; }
    
    /// <summary>
    /// Google ID for social login
    /// </summary>
    public string? GoogleId { get; set; }
    
    /// <summary>
    /// Microsoft ID for social login
    /// </summary>
    public string? MicrosoftId { get; set; }
    
    /// <summary>
    /// Whether user is new (created within last 7 days)
    /// </summary>
    public bool IsNewUser { get; set; }
    
    /// <summary>
    /// Whether user requires password change
    /// </summary>
    public bool RequiresPasswordChange { get; set; }
    
    /// <summary>
    /// Account creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Account created by
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Last modification date
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }
    
    /// <summary>
    /// Last modified by
    /// </summary>
    public string? LastModifiedBy { get; set; }
} 