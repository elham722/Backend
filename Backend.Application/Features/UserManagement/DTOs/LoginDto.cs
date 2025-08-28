namespace Backend.Application.Features.UserManagement.DTOs;

public class LoginDto
{
    public string EmailOrUsername { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; } = false;
  
    public string? TwoFactorCode { get; set; }
} 