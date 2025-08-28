namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for changing user password
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// User ID (if admin changing password for another user)
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// Current password (required for self-password change)
    /// </summary>
    public string? CurrentPassword { get; set; }
    
    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Confirm new password
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to require password change on next login
    /// </summary>
    public bool RequirePasswordChangeOnNextLogin { get; set; } = false;
} 