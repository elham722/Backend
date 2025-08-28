using System.Text.Json;

namespace Client.MVC.Services
{
    /// <summary>
    /// Interface for authenticated HTTP client with automatic token management
    /// </summary>
    public interface IAuthenticatedHttpClient
    {
        /// <summary>
        /// Send GET request with automatic authentication
        /// </summary>
        Task<TResponse?> GetAsync<TResponse>(string endpoint) where TResponse : class;

        /// <summary>
        /// Send POST request with automatic authentication
        /// </summary>
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request) 
            where TRequest : class 
            where TResponse : class;

        /// <summary>
        /// Send PUT request with automatic authentication
        /// </summary>
        Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request) 
            where TRequest : class 
            where TResponse : class;

        /// <summary>
        /// Send DELETE request with automatic authentication
        /// </summary>
        Task<bool> DeleteAsync(string endpoint);

        /// <summary>
        /// Send PATCH request with automatic authentication
        /// </summary>
        Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest request) 
            where TRequest : class 
            where TResponse : class;

        /// <summary>
        /// Add custom header to all requests
        /// </summary>
        void AddHeader(string name, string value);

        /// <summary>
        /// Remove custom header
        /// </summary>
        void RemoveHeader(string name);

        /// <summary>
        /// Clear all custom headers
        /// </summary>
        void ClearHeaders();
    }
} 