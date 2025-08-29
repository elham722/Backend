using Backend.Application.Features.UserManagement.DTOs;

namespace Client.MVC.Services
{
    /// <summary>
    /// Authentication client for background jobs and external services
    /// that need to validate tokens without going through the regular flow
    /// </summary>
    public interface IBackgroundJobAuthClient
    {
        /// <summary>
        /// Validate a JWT token (useful for background jobs, webhooks, etc.)
        /// Note: This is not needed for regular user requests as JWT tokens are self-contained
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>True if token is valid, false otherwise</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Validate current user's token from session/cookies
        /// </summary>
        /// <returns>True if current token is valid, false otherwise</returns>
        Task<bool> ValidateCurrentTokenAsync();

        /// <summary>
        /// Get token information without validation (for logging/debugging)
        /// </summary>
        /// <param name="token">JWT token to decode</param>
        /// <returns>Token information or null if invalid</returns>
        Task<object?> GetTokenInfoAsync(string token);
    }
} 