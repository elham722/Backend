namespace Backend.Application.Features.Auth.DTOs
{
    /// <summary>
    /// Strongly-typed response DTO for authentication operations
    /// </summary>
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
        public List<string>? Errors { get; set; }
    }

    /// <summary>
    /// DTO for token refresh requests
    /// </summary>
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for token refresh responses
    /// </summary>
    public class RefreshTokenResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Message { get; set; }
    }
} 