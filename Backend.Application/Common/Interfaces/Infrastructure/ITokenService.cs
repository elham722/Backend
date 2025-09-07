namespace Backend.Application.Common.Interfaces.Infrastructure
{
    /// <summary>
    /// Service interface for token operations (abstraction for Application layer)
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates an access token for the specified user
        /// </summary>
        Task<string> GenerateAccessTokenAsync(string userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Generates a refresh token
        /// </summary>
        Task<string> GenerateRefreshTokenAsync();
        
        /// <summary>
        /// Validates a refresh token
        /// </summary>
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Revokes a specific refresh token
        /// </summary>
        Task RevokeRefreshTokenAsync(string userId, string refreshToken, string revokedBy, string? reason = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Revokes all refresh tokens for a user
        /// </summary>
        Task RevokeAllRefreshTokensAsync(string userId, string revokedBy, string? reason = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets principal from expired token
        /// </summary>
        Task<System.Security.Claims.ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token);
    }
}