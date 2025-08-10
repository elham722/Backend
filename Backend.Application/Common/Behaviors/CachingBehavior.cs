using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Backend.Application.Common.Behaviors;

/// <summary>
/// Behavior for caching query results
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _slidingExpiration = TimeSpan.FromMinutes(5);

    public CachingBehavior(IMemoryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Only cache queries, not commands
        if (typeof(TRequest).Name.EndsWith("Query") == false)
        {
            return await next();
        }

        var cacheKey = GetCacheKey(request);
        
        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse))
        {
            _logger.LogInformation("Returning cached result for {RequestName}", typeof(TRequest).Name);
            return cachedResponse!;
        }

        var response = await next();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(_slidingExpiration);

        _cache.Set(cacheKey, response, cacheEntryOptions);
        
        _logger.LogInformation("Cached result for {RequestName}", typeof(TRequest).Name);
        
        return response;
    }

    private static string GetCacheKey(TRequest request)
    {
        var requestType = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request);
        return $"{requestType}_{requestJson.GetHashCode()}";
    }
} 