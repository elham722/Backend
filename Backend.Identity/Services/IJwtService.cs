using System.Security.Claims;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Interface for JWT token operations
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        string GenerateToken(string userId, string userName, string email, IEnumerable<string> roles);

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Validates a JWT token
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Validates a refresh token
        /// </summary>
        bool ValidateRefreshToken(string refreshToken);

        /// <summary>
        /// Gets the user ID from a token
        /// </summary>
        string? GetUserIdFromToken(string token);

        /// <summary>
        /// Gets the expiration time of a token
        /// </summary>
        DateTime GetTokenExpiration(string token);

        /// <summary>
        /// Checks if a token is expired
        /// </summary>
        bool IsTokenExpired(string token);

        /// <summary>
        /// Checks if a token is expiring soon
        /// </summary>
        bool IsTokenExpiringSoon(string token, int minutesThreshold = 5);
    }
} 