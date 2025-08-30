namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Service for caching JWT tokens and refresh tokens
/// </summary>
public interface ITokenCacheService
{
    /// <summary>
    /// Store JWT token in cache
    /// </summary>
    Task<bool> StoreJwtTokenAsync(string userId, string tokenId, string token, DateTime expiresAt);

    /// <summary>
    /// Store refresh token in cache
    /// </summary>
    Task<bool> StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt, string? deviceInfo = null, string? ipAddress = null);

    /// <summary>
    /// Get JWT token from cache
    /// </summary>
    Task<string?> GetJwtTokenAsync(string userId, string tokenId);

    /// <summary>
    /// Get refresh token from cache
    /// </summary>
    Task<string?> GetRefreshTokenAsync(string userId, string refreshToken);

    /// <summary>
    /// Invalidate JWT token
    /// </summary>
    Task<bool> InvalidateJwtTokenAsync(string userId, string tokenId);

    /// <summary>
    /// Invalidate refresh token
    /// </summary>
    Task<bool> InvalidateRefreshTokenAsync(string userId, string refreshToken);

    /// <summary>
    /// Invalidate all user tokens
    /// </summary>
    Task<bool> InvalidateAllUserTokensAsync(string userId);

    /// <summary>
    /// Check if JWT token is valid
    /// </summary>
    Task<bool> IsTokenValidAsync(string userId, string tokenId);

    /// <summary>
    /// Check if refresh token is valid
    /// </summary>
    Task<bool> IsRefreshTokenValidAsync(string userId, string refreshToken);

    /// <summary>
    /// Get active token count for user
    /// </summary>
    Task<int> GetActiveTokenCountAsync(string userId);

    /// <summary>
    /// Rotate tokens if needed (for security)
    /// </summary>
    Task<bool> RotateTokensIfNeededAsync(string userId, string currentTokenId, DateTime currentExpiry);

    /// <summary>
    /// Get token information
    /// </summary>
    Task<TokenInfo?> GetTokenInfoAsync(string userId, string tokenId);

    /// <summary>
    /// Get refresh token information
    /// </summary>
    Task<RefreshTokenInfo?> GetRefreshTokenInfoAsync(string userId, string refreshToken);

    /// <summary>
    /// Clean up expired tokens
    /// </summary>
    Task<int> CleanupExpiredTokensAsync();
}

/// <summary>
/// Token information model
/// </summary>
public class TokenInfo
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Refresh token information model
/// </summary>
public class RefreshTokenInfo
{
    public string UserId { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public string Type { get; set; } = string.Empty;
} 