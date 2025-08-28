namespace Backend.Application.Features.UserManagement.DTOs;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    
    public bool AcceptTerms { get; set; } = false;
    
    public bool SubscribeToNewsletter { get; set; } = false;
} 