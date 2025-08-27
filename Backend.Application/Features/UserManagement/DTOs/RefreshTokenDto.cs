namespace Backend.Application.Features.UserManagement.DTOs
{
    /// <summary>
    /// DTO for refresh token request
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
} 