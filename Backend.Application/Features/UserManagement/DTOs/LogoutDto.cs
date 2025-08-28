namespace Backend.Application.Features.UserManagement.DTOs
{
    public class LogoutDto
    {
        public string? RefreshToken { get; set; }
        
        public bool LogoutFromAllDevices { get; set; } = false;
    }
} 