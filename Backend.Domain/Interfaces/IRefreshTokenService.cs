namespace Backend.Domain.Interfaces
{
    /// <summary>
    /// Service for managing refresh tokens
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Invalidate a specific refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token to invalidate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if token was invalidated, false if not found or already invalid</returns>
        Task<bool> InvalidateTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidate all refresh tokens for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of tokens invalidated</returns>
        Task<int> InvalidateAllTokensForUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a refresh token is valid
        /// </summary>
        /// <param name="refreshToken">The refresh token to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if token is valid, false otherwise</returns>
        Task<bool> IsTokenValidAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user ID from refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User ID if token is valid, null otherwise</returns>
        Task<string?> GetUserIdFromTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Store a new refresh token
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="expiresAt">Token expiration time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if token was stored successfully</returns>
        Task<bool> StoreTokenAsync(string userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clean up expired tokens
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of tokens cleaned up</returns>
        Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
} 