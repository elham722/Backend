using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Backend.Infrastructure.Cache
{
    /// <summary>
    /// Configuration for Redis settings
    /// </summary>
    public class RedisConfiguration
    {
        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceName { get; set; } = "Backend:";
        public int DefaultDatabase { get; set; } = 0;
        public int ConnectTimeout { get; set; } = 5000;
        public int SyncTimeout { get; set; } = 5000;
        public int ConnectRetry { get; set; } = 3;
        public string ReconnectRetryPolicy { get; set; } = "LinearRetry";
        public int KeepAlive { get; set; } = 180;
    }

    /// <summary>
    /// Advanced Redis cache service for distributed caching
    /// </summary>
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        Task<bool> ExistsAsync(string key);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        Task ClearAllAsync();
        Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null);
        Task<double> IncrementAsync(string key, double value = 1, TimeSpan? expiration = null);
        Task<bool> LockAsync(string key, TimeSpan timeout);
        Task UnlockAsync(string key);
    }

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(value))
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default(T);
                }

                var result = JsonSerializer.Deserialize<T>(value, _jsonOptions);
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return default(T);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var jsonValue = JsonSerializer.Serialize(value, _jsonOptions);
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration;
                }
                else
                {
                    // Default expiration of 30 minutes
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }

                // Add sliding expiration for frequently accessed items
                options.SlidingExpiration = TimeSpan.FromMinutes(10);

                await _distributedCache.SetStringAsync(key, jsonValue, options);
                _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", 
                    key, expiration?.ToString() ?? "30 minutes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                // Note: IDistributedCache doesn't support pattern-based removal
                // This is a limitation - in a real Redis implementation, you would use SCAN
                _logger.LogWarning("Pattern-based cache removal not supported with IDistributedCache. Pattern: {Pattern}", pattern);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache values by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var value = await _distributedCache.GetStringAsync(key);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            try
            {
                var cachedValue = await GetAsync<T>(key);
                if (cachedValue != null)
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return cachedValue;
                }

                _logger.LogDebug("Cache miss for key: {Key}, executing factory", key);
                var value = await factory();
                await SetAsync(key, value, expiration);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrSet for key: {Key}", key);
                throw;
            }
        }

        public async Task ClearAllAsync()
        {
            try
            {
                // Note: IDistributedCache doesn't have a direct Clear method
                // This is a limitation - in a real Redis implementation, you would use FLUSHDB
                _logger.LogWarning("Clear all cache not supported with IDistributedCache");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
        }

        public async Task<long> IncrementAsync(string key, long value = 1, TimeSpan? expiration = null)
        {
            try
            {
                // Note: IDistributedCache doesn't support atomic increment
                // This is a limitation - in a real Redis implementation, you would use INCRBY
                _logger.LogWarning("Increment operations not fully supported with IDistributedCache. Key: {Key}", key);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing cache value for key: {Key}", key);
                return 0;
            }
        }

        public async Task<double> IncrementAsync(string key, double value = 1, TimeSpan? expiration = null)
        {
            try
            {
                // Note: IDistributedCache doesn't support atomic increment
                // This is a limitation - in a real Redis implementation, you would use INCRBYFLOAT
                _logger.LogWarning("Increment operations not fully supported with IDistributedCache. Key: {Key}", key);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing cache value for key: {Key}", key);
                return 0;
            }
        }

        public async Task<bool> LockAsync(string key, TimeSpan timeout)
        {
            try
            {
                // Simple lock implementation using cache
                var lockKey = $"lock:{key}";
                var lockValue = Guid.NewGuid().ToString();
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = timeout
                };

                await _distributedCache.SetStringAsync(lockKey, lockValue, options);
                
                // Verify we got the lock
                var acquiredValue = await _distributedCache.GetStringAsync(lockKey);
                return acquiredValue == lockValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring lock for key: {Key}", key);
                return false;
            }
        }

        public async Task UnlockAsync(string key)
        {
            try
            {
                var lockKey = $"lock:{key}";
                await _distributedCache.RemoveAsync(lockKey);
                _logger.LogDebug("Lock released for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lock for key: {Key}", key);
            }
        }
    }
} 