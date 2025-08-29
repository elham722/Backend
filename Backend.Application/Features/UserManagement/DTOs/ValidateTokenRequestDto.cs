namespace Backend.Application.Features.UserManagement.DTOs
{
    /// <summary>
    /// DTO for token validation request
    /// </summary>
    public class ValidateTokenRequestDto
    {
        /// <summary>
        /// JWT token to validate
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
} 