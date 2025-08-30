using System;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Backend.Application.Common.Interfaces;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Backend.Application.Common.Interfaces.Infrastructure;

namespace Backend.Identity.Services
{
    public interface IAccountManagementService
    {
        Task<bool> UpdateLastLoginAsync(ApplicationUser user);
        Task<bool> UpdatePasswordChangeAsync(ApplicationUser user);
        Task<bool> IncrementLoginAttemptsAsync(ApplicationUser user);
        Task<bool> DeactivateAccountAsync(ApplicationUser user);
        Task<bool> ActivateAccountAsync(ApplicationUser user);
        Task<bool> MarkAsDeletedAsync(ApplicationUser user);
        Task<bool> LinkToCustomerAsync(ApplicationUser user, Guid customerId);
        Task<bool> UnlinkFromCustomerAsync(ApplicationUser user);
        Task<List<Claim>> GetUserClaimsAsync(ApplicationUser user);
        Task<string> GenerateAccessTokenAsync(List<Claim> claims, string userId);
        Task<string> GenerateRefreshTokenAsync(string userId, string? deviceInfo = null, string? ipAddress = null);
        string GenerateAccessToken(List<Claim> claims); // For backward compatibility
        string GenerateRefreshToken(); // For backward compatibility
    }

    public class AccountManagementService : IAccountManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDateTimeService _dateTimeService;
        private readonly IConfiguration _configuration;
        private readonly ITokenCacheService _tokenCache;
        private readonly ILogger<AccountManagementService> _logger;

        public AccountManagementService(
            UserManager<ApplicationUser> userManager, 
            IDateTimeService dateTimeService,
            IConfiguration configuration,
            ITokenCacheService tokenCache,
            ILogger<AccountManagementService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> UpdateLastLoginAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.UpdateLastLogin(_dateTimeService);
            user.UpdateAccount(updatedAccount);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UpdatePasswordChangeAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.UpdatePasswordChange(_dateTimeService);
            user.UpdateAccount(updatedAccount);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> IncrementLoginAttemptsAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.IncrementLoginAttempts();
            user.UpdateAccount(updatedAccount);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeactivateAccountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.Deactivate();
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateAccount(updatedAccount);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ActivateAccountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.Activate();
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateAccount(updatedAccount);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> MarkAsDeletedAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.MarkAsDeleted(_dateTimeService);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateAccount(updatedAccount);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> LinkToCustomerAsync(ApplicationUser user, Guid customerId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetCustomerId(customerId);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UnlinkFromCustomerAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetCustomerId(null);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<List<Claim>> GetUserClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("UserId", user.Id),
                new Claim("UserName", user.UserName ?? string.Empty),
                new Claim("Email", user.Email ?? string.Empty),
                new Claim("EmailConfirmed", user.EmailConfirmed.ToString()),
                new Claim("IsActive", user.IsActive.ToString()),
                new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty),
                new Claim("PhoneNumberConfirmed", user.PhoneNumberConfirmed.ToString())
            };

            // Add roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        public async Task<string> GenerateAccessTokenAsync(List<Claim> claims, string userId)
        {
            try
            {
                var secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
                var issuer = _configuration["JwtSettings:Issuer"] ?? "Backend.Api";
                var audience = _configuration["JwtSettings:Audience"] ?? "Backend.Client";
                var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationInMinutes", 60);

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expiresAt,
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                
                // Generate unique token ID for caching
                var tokenId = Guid.NewGuid().ToString();
                
                // Store token in Redis cache
                var cacheSuccess = await _tokenCache.StoreJwtTokenAsync(userId, tokenId, tokenString, expiresAt);
                if (cacheSuccess)
                {
                    _logger.LogDebug("JWT token cached successfully for user: {UserId}, token: {TokenId}", userId, tokenId);
                }
                else
                {
                    _logger.LogWarning("Failed to cache JWT token for user: {UserId}", userId);
                }

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<string> GenerateRefreshTokenAsync(string userId, string? deviceInfo = null, string? ipAddress = null)
        {
            try
            {
                var refreshToken = Guid.NewGuid().ToString() + "_" + DateTime.UtcNow.Ticks;
                var expiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationInDays", 7));
                
                // Store refresh token in Redis cache
                var cacheSuccess = await _tokenCache.StoreRefreshTokenAsync(userId, refreshToken, expiresAt, deviceInfo, ipAddress);
                if (cacheSuccess)
                {
                    _logger.LogDebug("Refresh token cached successfully for user: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed to cache refresh token for user: {UserId}", userId);
                }

                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating refresh token for user: {UserId}", userId);
                throw;
            }
        }

        // Keep synchronous methods for backward compatibility
        public string GenerateAccessToken(List<Claim> claims)
        {
            // For backward compatibility, generate without caching
            var secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = _configuration["JwtSettings:Issuer"] ?? "Backend.Api";
            var audience = _configuration["JwtSettings:Audience"] ?? "Backend.Client";
            var expirationMinutes = _configuration.GetValue<int>("JwtSettings:ExpirationInMinutes", 60);

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            // For backward compatibility, generate without caching
            return Guid.NewGuid().ToString() + "_" + DateTime.UtcNow.Ticks;
        }
    }
} 