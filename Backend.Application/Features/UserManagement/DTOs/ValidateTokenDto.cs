namespace Backend.Application.Features.UserManagement.DTOs
{
    /// <summary>
    /// DTO for token validation response
    /// </summary>
    public class ValidateTokenDto
    {
        /// <summary>
        /// Whether token is valid
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// User information if token is valid
        /// </summary>
        public UserDto? User { get; set; }
        
        /// <summary>
        /// Error message if token is invalid
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
} 