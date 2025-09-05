namespace Client.MVC.Services.Abstractions
{
    public interface IConcurrencyManager
    {
        Task<T> ExecuteWithLockAsync<T>(string operationKey, Func<Task<T>> operation, TimeSpan? timeout = null);
        Task ExecuteWithLockAsync(string operationKey, Func<Task> operation, TimeSpan? timeout = null);
        Task<T> ExecuteWithLockAsync<T>(string operationKey, Func<Task<T>> operation, Func<Task<T>> fallbackOperation, TimeSpan? timeout = null);
        bool IsOperationInProgress(string operationKey);
        Task WaitForOperationCompletionAsync(string operationKey, TimeSpan? timeout = null);
        void CancelOperation(string operationKey);
    }
}
