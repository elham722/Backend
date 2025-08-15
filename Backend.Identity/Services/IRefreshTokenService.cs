namespace Backend.Identity.Services
{
    /// <summary>
    /// Interface for refresh token management
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Stores a refresh token for a user
        /// </summary>
        Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt);

        /// <summary>
        /// Validates a refresh token for a user
        /// </summary>
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);

        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        Task RevokeRefreshTokenAsync(string userId, string refreshToken);

        /// <summary>
        /// Revokes all refresh tokens for a user
        /// </summary>
        Task RevokeAllRefreshTokensAsync(string userId);

        /// <summary>
        /// Checks if a refresh token has been reused (security check)
        /// </summary>
        Task<bool> IsRefreshTokenReusedAsync(string userId, string refreshToken);

        /// <summary>
        /// Marks a refresh token as reused (for security)
        /// </summary>
        Task MarkRefreshTokenAsReusedAsync(string userId, string refreshToken);

        /// <summary>
        /// Cleans up expired refresh tokens
        /// </summary>
        Task CleanupExpiredTokensAsync();
    }
} 