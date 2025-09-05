using System.Net.Http.Headers;

namespace Client.MVC.Services.Abstractions
{
    /// <summary>
    /// Interface for authentication interceptor to handle JWT tokens automatically
    /// </summary>
    public interface IAuthenticationInterceptor
    {
        /// <summary>
        /// Add authentication header to the request
        /// </summary>
        Task<HttpRequestMessage> AddAuthenticationHeaderAsync(HttpRequestMessage request);

        /// <summary>
        /// Handle authentication errors (401, 403)
        /// </summary>
        Task<bool> HandleAuthenticationErrorAsync(HttpResponseMessage response);

        /// <summary>
        /// Refresh token if needed with thread-safety
        /// </summary>
        Task<bool> RefreshTokenIfNeededAsync();

        /// <summary>
        /// Wait for ongoing refresh operation to complete
        /// </summary>
        Task<bool> WaitForRefreshCompletionAsync(TimeSpan timeout = default);

        /// <summary>
        /// Check if token is valid
        /// </summary>
        bool IsTokenValid();

        /// <summary>
        /// Get current JWT token
        /// </summary>
        string? GetCurrentToken();

        /// <summary>
        /// Event raised when logout is required due to refresh token failure
        /// </summary>
        event EventHandler? OnLogoutRequired;
    }
} 