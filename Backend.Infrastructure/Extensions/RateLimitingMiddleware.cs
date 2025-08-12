using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Extensions
{
    /// <summary>
    /// Rate Limiting Middleware
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private const int MaxRequestsPerMinute = 100;
        private const int MaxRequestsPerHour = 1000;

        public RateLimitingMiddleware(
            RequestDelegate next, 
            IMemoryCache cache, 
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientId(context);
            
            if (!await IsRequestAllowed(clientId))
            {
                _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
                
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.Headers.Add("Retry-After", "60");
                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            await _next(context);
        }

        private string GetClientId(HttpContext context)
        {
            // Use IP address as client identifier
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // For authenticated users, you might want to use user ID instead
            var userId = context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user_{userId}";
            }
            
            return $"ip_{ipAddress}";
        }

        private async Task<bool> IsRequestAllowed(string clientId)
        {
            var now = DateTime.UtcNow;
            var minuteKey = $"rate_limit_minute_{clientId}_{now:yyyyMMddHHmm}";
            var hourKey = $"rate_limit_hour_{clientId}_{now:yyyyMMddHH}";

            // Check minute limit
            var minuteCount = await GetRequestCount(minuteKey);
            if (minuteCount >= MaxRequestsPerMinute)
            {
                return false;
            }

            // Check hour limit
            var hourCount = await GetRequestCount(hourKey);
            if (hourCount >= MaxRequestsPerHour)
            {
                return false;
            }

            // Increment counters
            await IncrementRequestCount(minuteKey, TimeSpan.FromMinutes(1));
            await IncrementRequestCount(hourKey, TimeSpan.FromHours(1));

            return true;
        }

        private async Task<int> GetRequestCount(string key)
        {
            return await Task.FromResult(_cache.Get<int>(key));
        }

        private async Task IncrementRequestCount(string key, TimeSpan expiration)
        {
            var count = _cache.Get<int>(key);
            count++;
            _cache.Set(key, count, expiration);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Extension method for adding rate limiting
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
} 