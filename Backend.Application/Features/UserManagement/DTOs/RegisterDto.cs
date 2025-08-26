namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for user registration
/// </summary>
public class RegisterDto
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
    /// User's password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Confirm password
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// User's phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Whether to accept terms and conditions
    /// </summary>
    public bool AcceptTerms { get; set; } = false;
    
    /// <summary>
    /// Whether to subscribe to newsletter
    /// </summary>
    public bool SubscribeToNewsletter { get; set; } = false;
} 