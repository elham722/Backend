namespace Backend.Infrastructure.ExternalServices
{
    public interface IExternalService
    {
        Task<TResponse> GetAsync<TResponse>(string endpoint, object query = null);
        Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);
        Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest request);
        Task DeleteAsync(string endpoint);
        Task<TResponse> PostAsync<TResponse>(string endpoint);
    }
} 