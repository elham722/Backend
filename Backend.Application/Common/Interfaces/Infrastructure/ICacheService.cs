namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Interface for caching services
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    /// <typeparam name="T">Type of the cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value or default</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Gets a value from cache asynchronously
    /// </summary>
    /// <typeparam name="T">Type of the cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value or default</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets a value in cache
    /// </summary>
    /// <typeparam name="T">Type of the value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expirationMinutes">Expiration time in minutes</param>
    void Set<T>(string key, T value, int expirationMinutes = 60);

    /// <summary>
    /// Sets a value in cache asynchronously
    /// </summary>
    /// <typeparam name="T">Type of the value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expirationMinutes">Expiration time in minutes</param>
    Task SetAsync<T>(string key, T value, int expirationMinutes = 60);

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    /// <param name="key">Cache key</param>
    void Remove(string key);

    /// <summary>
    /// Removes a value from cache asynchronously
    /// </summary>
    /// <param name="key">Cache key</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if a key exists in cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if key exists</returns>
    bool Exists(string key);

    /// <summary>
    /// Checks if a key exists in cache asynchronously
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if key exists</returns>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Gets or sets a value in cache
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to create value if not exists</param>
    /// <param name="expirationMinutes">Expiration time in minutes</param>
    /// <returns>Cached or newly created value</returns>
    T GetOrSet<T>(string key, Func<T> factory, int expirationMinutes = 60);

    /// <summary>
    /// Gets or sets a value in cache asynchronously
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="factory">Factory function to create value if not exists</param>
    /// <param name="expirationMinutes">Expiration time in minutes</param>
    /// <returns>Cached or newly created value</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int expirationMinutes = 60);
} 