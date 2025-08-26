namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for authentication result
/// </summary>
public class AuthResultDto
{
    /// <summary>
    /// Whether authentication was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Error message if authentication failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// JWT access token
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// JWT refresh token
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Token expiration date
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// User information
    /// </summary>
    public UserDto? User { get; set; }
    
    /// <summary>
    /// Whether two-factor authentication is required
    /// </summary>
    public bool RequiresTwoFactor { get; set; } = false;
    
    /// <summary>
    /// Whether email confirmation is required
    /// </summary>
    public bool RequiresEmailConfirmation { get; set; } = false;
    
    /// <summary>
    /// Whether password change is required
    /// </summary>
    public bool RequiresPasswordChange { get; set; } = false;
} 