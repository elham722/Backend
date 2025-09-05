namespace Client.MVC.Services.Abstractions
{
    /// <summary>
    /// Service for managing authentication tokens
    /// Follows Single Responsibility Principle - only handles token operations
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Get JWT token from secure storage
        /// </summary>
        /// <returns>JWT token if exists, null otherwise</returns>
        string? GetJwtToken();

        /// <summary>
        /// Get refresh token from secure storage
        /// </summary>
        /// <returns>Refresh token if exists, null otherwise</returns>
        string? GetRefreshToken();

        /// <summary>
        /// Check if JWT token is about to expire (within specified minutes)
        /// </summary>
        /// <param name="minutesBeforeExpiry">Minutes before expiry to consider token as "about to expire"</param>
        /// <returns>True if token expires within specified minutes, false otherwise</returns>
        bool IsTokenAboutToExpire(int minutesBeforeExpiry = 5);

        /// <summary>
        /// Get JWT token expiration time
        /// </summary>
        /// <returns>Token expiration time if valid, null otherwise</returns>
        DateTime? GetTokenExpiration();

        /// <summary>
        /// Get cached token expiration if available and valid
        /// </summary>
        /// <param name="currentToken">Current JWT token to validate against cache</param>
        /// <returns>Cached expiration time if valid, null otherwise</returns>
        DateTimeOffset? GetCachedTokenExpiration(string currentToken);

        /// <summary>
        /// Check if refresh token exists and is valid
        /// </summary>
        /// <returns>True if refresh token exists, false otherwise</returns>
        bool HasValidRefreshToken();

        /// <summary>
        /// Get refresh token expiration time (if it's a JWT token)
        /// </summary>
        /// <returns>Refresh token expiration time if it's a JWT, null otherwise</returns>
        DateTime? GetRefreshTokenExpiration();

        /// <summary>
        /// Check if refresh token is about to expire (within specified days)
        /// </summary>
        /// <param name="daysBeforeExpiry">Days before expiry to consider token as "about to expire"</param>
        /// <returns>True if token expires within specified days, false otherwise</returns>
        bool IsRefreshTokenAboutToExpire(int daysBeforeExpiry = 7);

        /// <summary>
        /// Get refresh token type (JWT or Opaque)
        /// </summary>
        /// <returns>Token type if valid, null otherwise</returns>
        string? GetRefreshTokenType();

        /// <summary>
        /// Refresh JWT token (called after token refresh)
        /// </summary>
        /// <param name="newToken">New JWT token</param>
        /// <param name="expiresAt">Token expiration time</param>
        void RefreshJwtToken(string newToken, DateTime? expiresAt = null);
    }
}