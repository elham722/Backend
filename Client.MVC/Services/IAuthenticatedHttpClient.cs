using System.Text.Json;

namespace Client.MVC.Services
{
    /// <summary>
    /// Interface for authenticated HTTP client with automatic token management and resilience policies
    /// </summary>
    public interface IAuthenticatedHttpClient
    {
        /// <summary>
        /// Send GET request with automatic authentication and resilience policies
        /// </summary>
        Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Send POST request with automatic authentication and resilience policies
        /// </summary>
        Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default) 
            where TRequest : class 
            where TResponse : class;

        /// <summary>
        /// Send PUT request with automatic authentication and resilience policies
        /// </summary>
        Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default) 
            where TRequest : class 
            where TResponse : class;

        /// <summary>
        /// Send DELETE request with automatic authentication and resilience policies
        /// </summary>
        Task<ApiResponse<bool>> DeleteAsync(string endpoint, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send PATCH request with automatic authentication and resilience policies
        /// </summary>
        Task<ApiResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default) 
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