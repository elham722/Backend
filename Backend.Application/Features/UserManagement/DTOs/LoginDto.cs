namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for user login
/// </summary>
public class LoginDto
{
    /// <summary>
    /// User's email or username
    /// </summary>
    public string EmailOrUsername { get; set; } = string.Empty;
    
    /// <summary>
    /// User's password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to remember the user
    /// </summary>
    public bool RememberMe { get; set; } = false;
    
    /// <summary>
    /// Two-factor authentication code (if enabled)
    /// </summary>
    public string? TwoFactorCode { get; set; }
} 