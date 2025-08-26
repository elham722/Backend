namespace Backend.Identity.DTOs
{
    /// <summary>
    /// Result of authentication operations
    /// </summary>
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<string>? Roles { get; set; }
        public List<string>? Errors { get; set; }
        public bool IsLockedOut { get; set; }
        public bool RequiresTwoFactor { get; set; }
    }
} 