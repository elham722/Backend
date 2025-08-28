using System.Net.Http.Headers;

namespace Client.MVC.Services
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
        /// Refresh token if needed
        /// </summary>
        Task<bool> RefreshTokenIfNeededAsync();

        /// <summary>
        /// Check if token is valid
        /// </summary>
        bool IsTokenValid();

        /// <summary>
        /// Get current JWT token
        /// </summary>
        string? GetCurrentToken();
    }
} 