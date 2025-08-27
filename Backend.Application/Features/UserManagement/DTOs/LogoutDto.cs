namespace Backend.Application.Features.UserManagement.DTOs
{
    /// <summary>
    /// DTO for logout request
    /// </summary>
    public class LogoutDto
    {
        /// <summary>
        /// Refresh token to invalidate (optional)
        /// </summary>
        public string? RefreshToken { get; set; }
        
        /// <summary>
        /// Whether to logout from all devices
        /// </summary>
        public bool LogoutFromAllDevices { get; set; } = false;
    }
} 