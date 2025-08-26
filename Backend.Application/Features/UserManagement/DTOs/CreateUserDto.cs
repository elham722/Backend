namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for creating a new user
/// </summary>
public class CreateUserDto
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
    /// User's phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// User roles to assign
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// Associated customer ID (optional)
    /// </summary>
    public Guid? CustomerId { get; set; }
    
    /// <summary>
    /// Whether to send confirmation email
    /// </summary>
    public bool SendConfirmationEmail { get; set; } = true;
    
    /// <summary>
    /// Whether to require password change on first login
    /// </summary>
    public bool RequirePasswordChange { get; set; } = false;
} 