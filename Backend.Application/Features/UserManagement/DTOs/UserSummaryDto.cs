using Backend.Application.Common.DTOs;
using System;

namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for user summary information
/// </summary>
public class UserSummaryDto : BaseDto
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
    /// Whether account is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Whether account is locked
    /// </summary>
    public bool IsLocked { get; set; }
    
    /// <summary>
    /// User roles
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// Account creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
}