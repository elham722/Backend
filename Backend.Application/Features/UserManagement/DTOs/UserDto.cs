using Backend.Application.Common.DTOs;

namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for user information
/// </summary>
public class UserDto : BaseDto
{
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
    public Guid? CustomerId { get; set; }
    
    /// <summary>
    /// Whether user is new (created within last 7 days)
    /// </summary>
    public bool IsNewUser { get; set; }
    
    /// <summary>
    /// Whether user requires password change
    /// </summary>
    public bool RequiresPasswordChange { get; set; }
} 