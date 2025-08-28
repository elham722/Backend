namespace Backend.Application.Features.UserManagement.DTOs;

public class AuthResultDto
{
    public bool IsSuccess { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public string? AccessToken { get; set; }
    
    public string? RefreshToken { get; set; }
   
    public DateTime? ExpiresAt { get; set; }
    
    public UserDto? User { get; set; }
    
    public bool RequiresTwoFactor { get; set; } = false;
    
    public bool RequiresEmailConfirmation { get; set; } = false;
    
    public bool RequiresPasswordChange { get; set; } = false;
} 