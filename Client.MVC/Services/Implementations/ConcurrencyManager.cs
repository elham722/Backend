using Client.MVC.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Client.MVC.Services.Implementations
{
    /// <summary>
    /// Advanced concurrency manager for handling concurrent operations safely
    /// </summary>
  

    public class ConcurrencyManager : IConcurrencyManager
    {
        private readonly ILogger<ConcurrencyManager> _logger;
        private readonly Dictionary<string, SemaphoreSlim> _locks;
        private readonly Dictionary<string, Task> _activeOperations;
        private readonly Dictionary<string, CancellationTokenSource> _cancellationTokens;
        private readonly object _dictionaryLock = new();

        public ConcurrencyManager(ILogger<ConcurrencyManager> logger)
        {
            _logger = logger;
            _locks = new Dictionary<string, SemaphoreSlim>();
            _activeOperations = new Dictionary<string, Task>();
            _cancellationTokens = new Dictionary<string, CancellationTokenSource>();
        }

        /// <summary>
        /// Execute an operation with a lock, ensuring only one instance runs at a time
        /// </summary>
        public async Task<T> ExecuteWithLockAsync<T>(string operationKey, Func<Task<T>> operation, TimeSpan? timeout = null)
        {
            var semaphore = GetOrCreateLock(operationKey);
            var timeoutValue = timeout ?? TimeSpan.FromSeconds(30);

            _logger.LogDebug("Attempting to acquire lock for operation: {OperationKey}", operationKey);

            if (!await semaphore.WaitAsync(timeoutValue))
            {
                _logger.LogWarning("Timeout waiting for lock on operation: {OperationKey}", operationKey);
                throw new TimeoutException($"Timeout waiting for lock on operation: {operationKey}");
            }

            try
            {
                _logger.LogDebug("Lock acquired for operation: {OperationKey}", operationKey);
                
                                // Check if operation is already running
                Task? existingTask = null;
                lock (_dictionaryLock)
                {
                    if (_activeOperations.TryGetValue(operationKey, out var task) && !task.IsCompleted)
                    {
                        _logger.LogDebug("Operation {OperationKey} already in progress, waiting for completion", operationKey);
                        existingTask = task;
                    }
                }

                if (existingTask != null)
                {
                    return await (Task<T>)existingTask;
                }

                // Execute the operation
                var operationTask = operation();
                
                lock (_dictionaryLock)
                {
                    _activeOperations[operationKey] = operationTask;
                }

                var result = await operationTask;
                _logger.LogDebug("Operation {OperationKey} completed successfully", operationKey);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing operation: {OperationKey}", operationKey);
                throw;
            }
            finally
            {
                lock (_dictionaryLock)
                {
                    _activeOperations.Remove(operationKey);
                }
                semaphore.Release();
                _logger.LogDebug("Lock released for operation: {OperationKey}", operationKey);
            }
        }

        /// <summary>
        /// Execute a void operation with a lock
        /// </summary>
        public async Task ExecuteWithLockAsync(string operationKey, Func<Task> operation, TimeSpan? timeout = null)
        {
            await ExecuteWithLockAsync(operationKey, async () =>
            {
                await operation();
                return true; // Dummy return value
            }, timeout);
        }

        /// <summary>
        /// Execute an operation with a lock and fallback operation
        /// </summary>
        public async Task<T> ExecuteWithLockAsync<T>(string operationKey, Func<Task<T>> operation, Func<Task<T>> fallbackOperation, TimeSpan? timeout = null)
        {
            try
            {
                return await ExecuteWithLockAsync(operationKey, operation, timeout);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Primary operation failed for {OperationKey}, attempting fallback", operationKey);
                return await fallbackOperation();
            }
        }

        /// <summary>
        /// Check if an operation is currently in progress
        /// </summary>
        public bool IsOperationInProgress(string operationKey)
        {
            lock (_dictionaryLock)
            {
                return _activeOperations.TryGetValue(operationKey, out var task) && !task.IsCompleted;
            }
        }

        /// <summary>
        /// Wait for an operation to complete
        /// </summary>
        public async Task WaitForOperationCompletionAsync(string operationKey, TimeSpan? timeout = null)
        {
            var timeoutValue = timeout ?? TimeSpan.FromSeconds(30);
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < timeoutValue)
            {
                lock (_dictionaryLock)
                {
                    if (!_activeOperations.TryGetValue(operationKey, out var task) || task.IsCompleted)
                    {
                        return;
                    }
                }

                await Task.Delay(100);
            }

            _logger.LogWarning("Timeout waiting for operation completion: {OperationKey}", operationKey);
            throw new TimeoutException($"Timeout waiting for operation completion: {operationKey}");
        }

        /// <summary>
        /// Cancel an ongoing operation
        /// </summary>
        public void CancelOperation(string operationKey)
        {
            lock (_dictionaryLock)
            {
                if (_cancellationTokens.TryGetValue(operationKey, out var cts))
                {
                    cts.Cancel();
                    _logger.LogInformation("Cancellation requested for operation: {OperationKey}", operationKey);
                }
            }
        }

        /// <summary>
        /// Get or create a semaphore for the operation key
        /// </summary>
        private SemaphoreSlim GetOrCreateLock(string operationKey)
        {
            lock (_dictionaryLock)
            {
                if (!_locks.TryGetValue(operationKey, out var semaphore))
                {
                    semaphore = new SemaphoreSlim(1, 1);
                    _locks[operationKey] = semaphore;
                    
                    // Create cancellation token source
                    if (!_cancellationTokens.ContainsKey(operationKey))
                    {
                        _cancellationTokens[operationKey] = new CancellationTokenSource();
                    }
                }
                return semaphore;
            }
        }

        /// <summary>
        /// Clean up resources for an operation key
        /// </summary>
        public void CleanupOperation(string operationKey)
        {
            lock (_dictionaryLock)
            {
                if (_locks.TryGetValue(operationKey, out var semaphore))
                {
                    semaphore.Dispose();
                    _locks.Remove(operationKey);
                }

                if (_cancellationTokens.TryGetValue(operationKey, out var cts))
                {
                    cts.Dispose();
                    _cancellationTokens.Remove(operationKey);
                }

                _activeOperations.Remove(operationKey);
            }
        }

        /// <summary>
        /// Get statistics about active operations
        /// </summary>
        public ConcurrencyStatistics GetStatistics()
        {
            lock (_dictionaryLock)
            {
                return new ConcurrencyStatistics
                {
                    TotalLocks = _locks.Count,
                    ActiveOperations = _activeOperations.Count(kvp => !kvp.Value.IsCompleted),
                    CompletedOperations = _activeOperations.Count(kvp => kvp.Value.IsCompleted)
                };
            }
        }
    }

    public class ConcurrencyStatistics
    {
        public int TotalLocks { get; set; }
        public int ActiveOperations { get; set; }
        public int CompletedOperations { get; set; }
    }
} 