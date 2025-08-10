using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Cache;

/// <summary>
/// Memory cache service implementation
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _defaultOptions;

    public MemoryCacheService(IMemoryCache memoryCache, IOptions<MemoryCacheOptions> options)
    {
        _memoryCache = memoryCache;
        _defaultOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.Value.DefaultExpirationMinutes)
        };
    }

    public T? Get<T>(string key)
    {
        return _memoryCache.Get<T>(key);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await Task.FromResult(_memoryCache.Get<T>(key));
    }

    public void Set<T>(string key, T value, int expirationMinutes = 60)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
        };
        _memoryCache.Set(key, value, options);
    }

    public async Task SetAsync<T>(string key, T value, int expirationMinutes = 60)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
        };
        _memoryCache.Set(key, value, options);
        await Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        await Task.CompletedTask;
    }

    public bool Exists(string key)
    {
        return _memoryCache.TryGetValue(key, out _);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    public T GetOrSet<T>(string key, Func<T> factory, int expirationMinutes = 60)
    {
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue!;
        }

        var value = factory();
        Set(key, value, expirationMinutes);
        return value;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int expirationMinutes = 60)
    {
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue!;
        }

        var value = await factory();
        await SetAsync(key, value, expirationMinutes);
        return value;
    }
}

/// <summary>
/// Memory cache options
/// </summary>
public class MemoryCacheOptions
{
    public int DefaultExpirationMinutes { get; set; } = 60;
    public int SizeLimit { get; set; } = 1000;
} 