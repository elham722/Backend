namespace Client.MVC.Models
{
    /// <summary>
    /// DTO for token validation response
    /// </summary>
    public class ValidateTokenDto
    {
        /// <summary>
        /// Indicates if the token is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if validation fails
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Token expiration time
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
} 