using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Implementation of refresh token management service
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly int _refreshTokenExpirationDays;
        private readonly int _maxTokensPerUser;

        public RefreshTokenService(
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<RefreshTokenService> logger)
        {
            _cache = cache;
            _logger = logger;
            _refreshTokenExpirationDays = int.Parse(configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7");
            _maxTokensPerUser = int.Parse(configuration["JwtSettings:MaxRefreshTokensPerUser"] ?? "5");
        }

        public async Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt)
        {
            try
            {
                var tokenKey = GetTokenKey(userId, refreshToken);
                var userTokensKey = GetUserTokensKey(userId);

                // Store the refresh token
                var tokenInfo = new RefreshTokenInfo
                {
                    Token = refreshToken,
                    UserId = userId,
                    ExpiresAt = expiresAt,
                    IsRevoked = false,
                    IsReused = false,
                    CreatedAt = DateTime.UtcNow
                };

                _cache.Set(tokenKey, tokenInfo, expiresAt);

                // Store in user's token list for management
                var userTokens = await GetUserTokensAsync(userId);
                userTokens.Add(tokenInfo);
                
                // Limit the number of tokens per user
                if (userTokens.Count > _maxTokensPerUser)
                {
                    var oldestToken = userTokens.OrderBy(t => t.CreatedAt).First();
                    userTokens.Remove(oldestToken);
                    _cache.Remove(GetTokenKey(userId, oldestToken.Token));
                    _logger.LogInformation("Removed oldest refresh token for user {UserId}", userId);
                }

                _cache.Set(userTokensKey, userTokens, expiresAt);

                _logger.LogDebug("Stored refresh token for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing refresh token for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            try
            {
                var tokenKey = GetTokenKey(userId, refreshToken);
                var tokenInfo = _cache.Get<RefreshTokenInfo>(tokenKey);

                if (tokenInfo == null)
                {
                    _logger.LogWarning("Refresh token not found for user {UserId}", userId);
                    return false;
                }

                if (tokenInfo.IsRevoked)
                {
                    _logger.LogWarning("Refresh token is revoked for user {UserId}", userId);
                    return false;
                }

                if (tokenInfo.IsReused)
                {
                    _logger.LogWarning("Refresh token has been reused for user {UserId}", userId);
                    return false;
                }

                if (tokenInfo.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token has expired for user {UserId}", userId);
                    await RevokeRefreshTokenAsync(userId, refreshToken);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token for user {UserId}", userId);
                return false;
            }
        }

        public async Task RevokeRefreshTokenAsync(string userId, string refreshToken)
        {
            try
            {
                var tokenKey = GetTokenKey(userId, refreshToken);
                var tokenInfo = _cache.Get<RefreshTokenInfo>(tokenKey);

                if (tokenInfo != null)
                {
                    tokenInfo.IsRevoked = true;
                    _cache.Set(tokenKey, tokenInfo, tokenInfo.ExpiresAt);

                    // Remove from user's token list
                    var userTokens = await GetUserTokensAsync(userId);
                    var tokenToRemove = userTokens.FirstOrDefault(t => t.Token == refreshToken);
                    if (tokenToRemove != null)
                    {
                        userTokens.Remove(tokenToRemove);
                        _cache.Set(GetUserTokensKey(userId), userTokens, tokenInfo.ExpiresAt);
                    }

                    _logger.LogInformation("Revoked refresh token for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token for user {UserId}", userId);
                throw;
            }
        }

        public async Task RevokeAllRefreshTokensAsync(string userId)
        {
            try
            {
                var userTokens = await GetUserTokensAsync(userId);
                
                foreach (var token in userTokens)
                {
                    var tokenKey = GetTokenKey(userId, token.Token);
                    token.IsRevoked = true;
                    _cache.Set(tokenKey, token, token.ExpiresAt);
                }

                _cache.Remove(GetUserTokensKey(userId));
                _logger.LogInformation("Revoked all refresh tokens for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all refresh tokens for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsRefreshTokenReusedAsync(string userId, string refreshToken)
        {
            try
            {
                var tokenKey = GetTokenKey(userId, refreshToken);
                var tokenInfo = _cache.Get<RefreshTokenInfo>(tokenKey);

                return tokenInfo?.IsReused ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking refresh token reuse for user {UserId}", userId);
                return false;
            }
        }

        public async Task MarkRefreshTokenAsReusedAsync(string userId, string refreshToken)
        {
            try
            {
                var tokenKey = GetTokenKey(userId, refreshToken);
                var tokenInfo = _cache.Get<RefreshTokenInfo>(tokenKey);

                if (tokenInfo != null)
                {
                    tokenInfo.IsReused = true;
                    _cache.Set(tokenKey, tokenInfo, tokenInfo.ExpiresAt);

                    // Revoke all other tokens for this user (security measure)
                    await RevokeAllRefreshTokensAsync(userId);

                    _logger.LogWarning("Marked refresh token as reused and revoked all tokens for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking refresh token as reused for user {UserId}", userId);
                throw;
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                // This is a simplified cleanup - in production, you might want to use a background service
                // or database-based storage for better cleanup management
                _logger.LogInformation("Starting cleanup of expired refresh tokens");
                
                // Note: Memory cache automatically removes expired items
                // This method is mainly for logging and future database implementations
                
                _logger.LogInformation("Cleanup of expired refresh tokens completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup of expired refresh tokens");
                throw;
            }
        }

        private async Task<List<RefreshTokenInfo>> GetUserTokensAsync(string userId)
        {
            var userTokensKey = GetUserTokensKey(userId);
            var userTokens = _cache.Get<List<RefreshTokenInfo>>(userTokensKey) ?? new List<RefreshTokenInfo>();
            
            // Remove expired tokens
            userTokens.RemoveAll(t => t.ExpiresAt <= DateTime.UtcNow);
            
            return userTokens;
        }

        private static string GetTokenKey(string userId, string refreshToken)
        {
            return $"refresh_token:{userId}:{refreshToken}";
        }

        private static string GetUserTokensKey(string userId)
        {
            return $"user_tokens:{userId}";
        }

        private class RefreshTokenInfo
        {
            public string Token { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public bool IsRevoked { get; set; }
            public bool IsReused { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
} 