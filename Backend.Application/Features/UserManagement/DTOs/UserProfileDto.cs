namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for user profile information (self-updatable)
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// User's username
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// User's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Current password (required for sensitive changes)
    /// </summary>
    public string? CurrentPassword { get; set; }
    
    /// <summary>
    /// New password (optional)
    /// </summary>
    public string? NewPassword { get; set; }
    
    /// <summary>
    /// Confirm new password
    /// </summary>
    public string? ConfirmNewPassword { get; set; }
} 