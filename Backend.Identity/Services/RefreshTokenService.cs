using Backend.Identity.Context;
using Backend.Domain.Interfaces;
using Backend.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Service for managing refresh tokens
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly BackendIdentityDbContext _context;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(
            BackendIdentityDbContext context,
            ILogger<RefreshTokenService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invalidate a specific refresh token
        /// </summary>
        public async Task<bool> InvalidateTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Cannot invalidate null or empty refresh token");
                    return false;
                }

                // Find the token in database
                var tokenEntity = await _context.UserTokens
                    .FirstOrDefaultAsync(t => t.Value == refreshToken, cancellationToken);

                if (tokenEntity == null)
                {
                    _logger.LogWarning("Refresh token not found in database: {TokenPrefix}", 
                        refreshToken.Substring(0, Math.Min(10, refreshToken.Length)));
                    return false;
                }

                // Mark token as invalid by setting it to null or empty
                tokenEntity.Value = null;
                tokenEntity.Name = "INVALIDATED";

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully invalidated refresh token for user: {UserId}", 
                    tokenEntity.UserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating refresh token");
                return false;
            }
        }

        /// <summary>
        /// Invalidate all refresh tokens for a specific user
        /// </summary>
        public async Task<int> InvalidateAllTokensForUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Cannot invalidate tokens for null or empty user ID");
                    return 0;
                }

                // Find all refresh tokens for the user
                var userTokens = await _context.UserTokens
                    .Where(t => t.UserId == userId && t.Name == "RefreshToken")
                    .ToListAsync(cancellationToken);

                if (!userTokens.Any())
                {
                    _logger.LogInformation("No refresh tokens found for user: {UserId}", userId);
                    return 0;
                }

                // Invalidate all tokens
                foreach (var token in userTokens)
                {
                    token.Value = null;
                    token.Name = "INVALIDATED";
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully invalidated {TokenCount} refresh tokens for user: {UserId}", 
                    userTokens.Count, userId);

                return userTokens.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating all refresh tokens for user: {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// Check if a refresh token is valid
        /// </summary>
        public async Task<bool> IsTokenValidAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return false;
                }

                // Check if token exists and is not invalidated
                var tokenEntity = await _context.UserTokens
                    .FirstOrDefaultAsync(t => t.Value == refreshToken && t.Name == "RefreshToken", cancellationToken);

                if (tokenEntity == null)
                {
                    return false;
                }

                // Check if token is expired (if it's a JWT token)
                if (IsJwtToken(refreshToken))
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(refreshToken))
                    {
                        var token = handler.ReadJwtToken(refreshToken);
                        if (token.ValidTo <= DateTime.UtcNow)
                        {
                            _logger.LogDebug("Refresh token is expired: {TokenPrefix}", 
                                refreshToken.Substring(0, Math.Min(10, refreshToken.Length)));
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking refresh token validity");
                return false;
            }
        }

        /// <summary>
        /// Get user ID from refresh token
        /// </summary>
        public async Task<string?> GetUserIdFromTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return null;
                }

                // First try to get from database
                var tokenEntity = await _context.UserTokens
                    .FirstOrDefaultAsync(t => t.Value == refreshToken && t.Name == "RefreshToken", cancellationToken);

                if (tokenEntity != null)
                {
                    return tokenEntity.UserId;
                }

                // If not found in database, try to extract from JWT token
                if (IsJwtToken(refreshToken))
                {
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(refreshToken))
                    {
                        var token = handler.ReadJwtToken(refreshToken);
                        return token.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "nameid")?.Value;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID from refresh token");
                return null;
            }
        }

        /// <summary>
        /// Store a new refresh token
        /// </summary>
        public async Task<bool> StoreTokenAsync(string userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Cannot store token: UserId or RefreshToken is null/empty");
                    return false;
                }

                // Check if user exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
                if (!userExists)
                {
                    _logger.LogWarning("Cannot store token: User not found: {UserId}", userId);
                    return false;
                }

                // Create new token entity
                var tokenEntity = UserToken.Create(
                    userId: userId,
                    loginProvider: "JWT",
                    name: "RefreshToken",
                    value: refreshToken,
                    expiresAt: expiresAt,
                    createdBy: "System"
                );

                _context.UserTokens.Add(tokenEntity);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully stored refresh token for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing refresh token for user: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Clean up expired tokens
        /// </summary>
        public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var expiredTokens = new List<Backend.Identity.Models.UserToken>();

                // Get all refresh tokens
                var allTokens = await _context.UserTokens
                    .Where(t => t.Name == "RefreshToken" && !string.IsNullOrEmpty(t.Value))
                    .ToListAsync(cancellationToken);

                foreach (var token in allTokens)
                {
                    if (IsJwtToken(token.Value))
                    {
                        var handler = new JwtSecurityTokenHandler();
                        if (handler.CanReadToken(token.Value))
                        {
                            var jwtToken = handler.ReadJwtToken(token.Value);
                            if (jwtToken.ValidTo <= DateTime.UtcNow)
                            {
                                expiredTokens.Add((Backend.Identity.Models.UserToken)token);
                            }
                        }
                    }
                }

                if (expiredTokens.Any())
                {
                    // Mark expired tokens as invalidated
                    foreach (var token in expiredTokens)
                    {
                        token.Value = null;
                        token.Name = "EXPIRED";
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Cleaned up {TokenCount} expired refresh tokens", expiredTokens.Count);
                }

                return expiredTokens.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired tokens");
                return 0;
            }
        }

        /// <summary>
        /// Check if token is a JWT token
        /// </summary>
        private bool IsJwtToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return false;

                // JWT tokens have 3 parts separated by dots
                var parts = token.Split('.');
                return parts.Length == 3;
            }
            catch
            {
                return false;
            }
        }
    }
} 