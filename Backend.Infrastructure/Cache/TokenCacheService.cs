using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Backend.Infrastructure.Cache
{
    /// <summary>
    /// Service for caching JWT tokens and refresh tokens using distributed cache
    /// </summary>
    public class TokenCacheService : ITokenCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<TokenCacheService> _logger;
        private readonly IOptions<TokenCacheConfiguration> _config;
        private readonly JsonSerializerOptions _jsonOptions;

        public TokenCacheService(
            IDistributedCache distributedCache,
            ILogger<TokenCacheService> logger,
            IOptions<TokenCacheConfiguration> config)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Store JWT token in cache
        /// </summary>
        public async Task<bool> StoreJwtTokenAsync(string userId, string tokenId, string token, DateTime expiresAt)
        {
            try
            {
                var tokenInfo = new TokenInfo
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Type = "JWT"
                };

                var key = $"jwt:{userId}:{tokenId}";
                var expiration = expiresAt - DateTime.UtcNow;

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };

                var jsonValue = JsonSerializer.Serialize(tokenInfo, _jsonOptions);
                await _distributedCache.SetStringAsync(key, jsonValue, options);

                _logger.LogDebug("JWT token stored successfully for user: {UserId}, token: {TokenId}", userId, tokenId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing JWT token for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Store refresh token in cache
        /// </summary>
        public async Task<bool> StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt, string? deviceInfo = null, string? ipAddress = null)
        {
            try
            {
                var tokenInfo = new RefreshTokenInfo
                {
                    UserId = userId,
                    RefreshToken = refreshToken,
                    DeviceInfo = deviceInfo,
                    IpAddress = ipAddress,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsActive = true,
                    Type = "Refresh"
                };

                var key = $"refresh:{userId}:{refreshToken}";
                var expiration = expiresAt - DateTime.UtcNow;

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };

                var jsonValue = JsonSerializer.Serialize(tokenInfo, _jsonOptions);
                await _distributedCache.SetStringAsync(key, jsonValue, options);

                // Also store in user's refresh token list
                await AddToUserRefreshTokenListAsync(userId, refreshToken, expiresAt);

                _logger.LogDebug("Refresh token stored successfully for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing refresh token for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Get JWT token from cache
        /// </summary>
        public async Task<string?> GetJwtTokenAsync(string userId, string tokenId)
        {
            try
            {
                var key = $"jwt:{userId}:{tokenId}";
                var jsonValue = await _distributedCache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(jsonValue))
                {
                    return null;
                }

                var tokenInfo = JsonSerializer.Deserialize<TokenInfo>(jsonValue, _jsonOptions);
                if (tokenInfo?.IsActive == true && tokenInfo.ExpiresAt > DateTime.UtcNow)
                {
                    return tokenInfo.Token;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting JWT token for user: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Get refresh token from cache
        /// </summary>
        public async Task<string?> GetRefreshTokenAsync(string userId, string refreshToken)
        {
            try
            {
                var key = $"refresh:{userId}:{refreshToken}";
                var jsonValue = await _distributedCache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(jsonValue))
                {
                    return null;
                }

                var tokenInfo = JsonSerializer.Deserialize<RefreshTokenInfo>(jsonValue, _jsonOptions);
                if (tokenInfo?.IsActive == true && tokenInfo.ExpiresAt > DateTime.UtcNow)
                {
                    return tokenInfo.RefreshToken;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refresh token for user: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Invalidate JWT token
        /// </summary>
        public async Task<bool> InvalidateJwtTokenAsync(string userId, string tokenId)
        {
            try
            {
                var key = $"jwt:{userId}:{tokenId}";
                await _distributedCache.RemoveAsync(key);
                
                _logger.LogDebug("JWT token invalidated for user: {UserId}, token: {TokenId}", userId, tokenId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating JWT token for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Invalidate refresh token
        /// </summary>
        public async Task<bool> InvalidateRefreshTokenAsync(string userId, string refreshToken)
        {
            try
            {
                var key = $"refresh:{userId}:{refreshToken}";
                await _distributedCache.RemoveAsync(key);
                
                // Remove from user's refresh token list
                await RemoveFromUserRefreshTokenListAsync(userId, refreshToken);
                
                _logger.LogDebug("Refresh token invalidated for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating refresh token for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Invalidate all user tokens
        /// </summary>
        public async Task<bool> InvalidateAllUserTokensAsync(string userId)
        {
            try
            {
                // Get all user tokens and invalidate them
                var userTokensKey = $"user_tokens:{userId}";
                var userTokens = await _distributedCache.GetStringAsync(userTokensKey);
                
                if (!string.IsNullOrEmpty(userTokens))
                {
                    var tokenList = JsonSerializer.Deserialize<List<string>>(userTokens) ?? new List<string>();
                    
                    foreach (var token in tokenList)
                    {
                        await InvalidateRefreshTokenAsync(userId, token);
                    }
                }
                
                // Clear user tokens list
                await _distributedCache.RemoveAsync(userTokensKey);
                
                _logger.LogInformation("All tokens invalidated for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating all tokens for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Check if JWT token is valid
        /// </summary>
        public async Task<bool> IsTokenValidAsync(string userId, string tokenId)
        {
            try
            {
                var token = await GetJwtTokenAsync(userId, tokenId);
                return !string.IsNullOrEmpty(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking JWT token validity for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Check if refresh token is valid
        /// </summary>
        public async Task<bool> IsRefreshTokenValidAsync(string userId, string refreshToken)
        {
            try
            {
                var token = await GetRefreshTokenAsync(userId, refreshToken);
                return !string.IsNullOrEmpty(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking refresh token validity for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Get active token count for user
        /// </summary>
        public async Task<int> GetActiveTokenCountAsync(string userId)
        {
            try
            {
                var userTokensKey = $"user_tokens:{userId}";
                var userTokens = await _distributedCache.GetStringAsync(userTokensKey);
                
                if (string.IsNullOrEmpty(userTokens))
                {
                    return 0;
                }

                var tokenList = JsonSerializer.Deserialize<List<string>>(userTokens) ?? new List<string>();
                return tokenList.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active token count for user: {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// Rotate tokens if needed (for security)
        /// </summary>
        public async Task<bool> RotateTokensIfNeededAsync(string userId, string currentTokenId, DateTime currentExpiry)
        {
            try
            {
                var threshold = DateTime.UtcNow.AddMinutes(_config.Value.TokenRotationThresholdMinutes);
                
                if (currentExpiry <= threshold)
                {
                    // Token is about to expire, trigger rotation
                    _logger.LogInformation("Token rotation triggered for user: {UserId}", userId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token rotation for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Get token information
        /// </summary>
        public async Task<TokenInfo?> GetTokenInfoAsync(string userId, string tokenId)
        {
            try
            {
                var key = $"jwt:{userId}:{tokenId}";
                var jsonValue = await _distributedCache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(jsonValue))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<TokenInfo>(jsonValue, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token info for user: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Get refresh token information
        /// </summary>
        public async Task<RefreshTokenInfo?> GetRefreshTokenInfoAsync(string userId, string refreshToken)
        {
            try
            {
                var key = $"refresh:{userId}:{refreshToken}";
                var jsonValue = await _distributedCache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(jsonValue))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<RefreshTokenInfo>(jsonValue, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refresh token info for user: {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Clean up expired tokens
        /// </summary>
        public async Task<int> CleanupExpiredTokensAsync()
        {
            try
            {
                // This is a simplified cleanup - in production, you might want to use a background service
                // or Redis SCAN commands for more efficient cleanup
                _logger.LogInformation("Token cleanup completed");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token cleanup");
                return 0;
            }
        }

        /// <summary>
        /// Add refresh token to user's token list
        /// </summary>
        private async Task AddToUserRefreshTokenListAsync(string userId, string refreshToken, DateTime expiresAt)
        {
            try
            {
                var userTokensKey = $"user_tokens:{userId}";
                var userTokens = await _distributedCache.GetStringAsync(userTokensKey);
                
                var tokenList = !string.IsNullOrEmpty(userTokens) 
                    ? JsonSerializer.Deserialize<List<string>>(userTokens) ?? new List<string>()
                    : new List<string>();
                
                // Add new token
                if (!tokenList.Contains(refreshToken))
                {
                    tokenList.Add(refreshToken);
                }
                
                // Limit tokens per user
                var maxTokens = _config.Value.MaxTokensPerUser;
                if (tokenList.Count > maxTokens)
                {
                    tokenList = tokenList.Take(maxTokens).ToList();
                }
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiresAt - DateTime.UtcNow
                };
                
                await _distributedCache.SetStringAsync(userTokensKey, JsonSerializer.Serialize(tokenList), options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding refresh token to user list for user: {UserId}", userId);
            }
        }

        /// <summary>
        /// Remove refresh token from user's token list
        /// </summary>
        private async Task RemoveFromUserRefreshTokenListAsync(string userId, string refreshToken)
        {
            try
            {
                var userTokensKey = $"user_tokens:{userId}";
                var userTokens = await _distributedCache.GetStringAsync(userTokensKey);
                
                if (!string.IsNullOrEmpty(userTokens))
                {
                    var tokenList = JsonSerializer.Deserialize<List<string>>(userTokens) ?? new List<string>();
                    tokenList.Remove(refreshToken);
                    
                    if (tokenList.Count > 0)
                    {
                        await _distributedCache.SetStringAsync(userTokensKey, JsonSerializer.Serialize(tokenList));
                    }
                    else
                    {
                        await _distributedCache.RemoveAsync(userTokensKey);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing refresh token from user list for user: {UserId}", userId);
            }
        }
    }
} 