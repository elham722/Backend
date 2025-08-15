using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Backend.Infrastructure.Extensions
{
    /// <summary>
    /// Middleware for rate limiting requests
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly RateLimitingOptions _options;

        public RateLimitingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<RateLimitingMiddleware> logger,
            RateLimitingOptions options)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var endpoint = GetEndpointIdentifier(context);

            if (await IsRateLimitExceeded(clientId, endpoint))
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", clientId, endpoint);
                await ReturnRateLimitExceededResponse(context);
                return;
            }

            await _next(context);
        }

        private static string GetClientIdentifier(HttpContext context)
        {
            // Try to get from X-Forwarded-For header first (for proxy scenarios)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Fall back to remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private static string GetEndpointIdentifier(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "/";
            var method = context.Request.Method;
            return $"{method}:{path}";
        }

        private async Task<bool> IsRateLimitExceeded(string clientId, string endpoint)
        {
            var key = $"rate_limit:{clientId}:{endpoint}";
            var windowKey = $"rate_limit_window:{clientId}:{endpoint}";

            var currentTime = DateTime.UtcNow;
            var windowStart = currentTime.AddMinutes(-_options.WindowMinutes);

            // Get or create rate limit info
            var rateLimitInfo = await GetOrCreateRateLimitInfo(key, windowKey, windowStart);

            // Check if we're in a new window
            if (rateLimitInfo.WindowStart < windowStart)
            {
                rateLimitInfo.RequestCount = 0;
                rateLimitInfo.WindowStart = currentTime;
            }

            // Increment request count
            rateLimitInfo.RequestCount++;

            // Check if limit exceeded
            var isExceeded = rateLimitInfo.RequestCount > _options.MaxRequestsPerWindow;

            // Update cache
            var expiration = currentTime.AddMinutes(_options.WindowMinutes);
            _cache.Set(key, rateLimitInfo, expiration);
            _cache.Set(windowKey, rateLimitInfo.WindowStart, expiration);

            // Log if approaching limit
            if (rateLimitInfo.RequestCount >= _options.MaxRequestsPerWindow * 0.8)
            {
                _logger.LogInformation("Rate limit approaching for client {ClientId} on endpoint {Endpoint}: {Count}/{Max}",
                    clientId, endpoint, rateLimitInfo.RequestCount, _options.MaxRequestsPerWindow);
            }

            return isExceeded;
        }

        private async Task<RateLimitInfo> GetOrCreateRateLimitInfo(string key, string windowKey, DateTime windowStart)
        {
            var rateLimitInfo = _cache.Get<RateLimitInfo>(key);
            if (rateLimitInfo == null)
            {
                rateLimitInfo = new RateLimitInfo
                {
                    RequestCount = 0,
                    WindowStart = windowStart
                };
            }

            return rateLimitInfo;
        }

        private static async Task ReturnRateLimitExceededResponse(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Rate limit exceeded",
                message = "Too many requests. Please try again later.",
                retryAfter = 60 // seconds
            };

            // Add Retry-After header
            context.Response.Headers.Add("Retry-After", "60");

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }

        private class RateLimitInfo
        {
            public int RequestCount { get; set; }
            public DateTime WindowStart { get; set; }
        }
    }

    /// <summary>
    /// Options for rate limiting
    /// </summary>
    public class RateLimitingOptions
    {
        public int MaxRequestsPerWindow { get; set; } = 100;
        public int WindowMinutes { get; set; } = 1;
        public bool EnableLogging { get; set; } = true;
    }
} 