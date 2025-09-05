using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Client.MVC.Services.Abstractions;

namespace Client.MVC.Services.Implementations
{
  

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly Dictionary<string, DateTime> _keyTimestamps;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _keyTimestamps = new Dictionary<string, DateTime>();
        }

        public Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out var value))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return Task.FromResult((T?)value);
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return Task.FromResult(default(T));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return Task.FromResult(default(T));
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                
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

                _memoryCache.Set(key, value, options);
                _keyTimestamps[key] = DateTime.UtcNow;

                _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", 
                    key, expiration?.ToString() ?? "30 minutes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
            
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _keyTimestamps.Remove(key);
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
            
            return Task.CompletedTask;
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var keysToRemove = _keyTimestamps.Keys
                    .Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _keyTimestamps.Remove(key);
                }

                _logger.LogDebug("Cache removed {Count} items matching pattern: {Pattern}", 
                    keysToRemove.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache values by pattern: {Pattern}", pattern);
            }
            
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                return Task.FromResult(_memoryCache.TryGetValue(key, out _));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return Task.FromResult(false);
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out var cachedValue))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return (T)cachedValue;
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

        public Task ClearAllAsync()
        {
            try
            {
                // Note: IMemoryCache doesn't have a direct Clear method
                // This is a workaround by removing all tracked keys
                var keysToRemove = _keyTimestamps.Keys.ToList();
                
                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                }
                
                _keyTimestamps.Clear();
                _logger.LogInformation("Cache cleared. Removed {Count} items", keysToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
            
            return Task.CompletedTask;
        }

        // Helper method to get cache statistics
        public CacheStatistics GetStatistics()
        {
            return new CacheStatistics
            {
                TotalKeys = _keyTimestamps.Count,
                OldestKey = _keyTimestamps.Values.Any() ? _keyTimestamps.Values.Min() : DateTime.UtcNow,
                NewestKey = _keyTimestamps.Values.Any() ? _keyTimestamps.Values.Max() : DateTime.UtcNow
            };
        }
    }

    public class CacheStatistics
    {
        public int TotalKeys { get; set; }
        public DateTime OldestKey { get; set; }
        public DateTime NewestKey { get; set; }
    }
} 