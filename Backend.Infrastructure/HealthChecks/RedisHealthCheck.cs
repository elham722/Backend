using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Backend.Infrastructure.Cache;

namespace Backend.Infrastructure.HealthChecks
{
    /// <summary>
    /// Health check for distributed cache service (Redis/Memory)
    /// </summary>
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisHealthCheck> _logger;
        private readonly RedisConfiguration _redisConfig;

        public RedisHealthCheck(
            IDistributedCache distributedCache,
            ILogger<RedisHealthCheck> logger,
            IOptions<RedisConfiguration> redisConfig)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _redisConfig = redisConfig?.Value ?? throw new ArgumentNullException(nameof(redisConfig));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Test distributed cache connection by setting and getting a test value
                var testKey = $"health_check:{Guid.NewGuid()}";
                var testValue = DateTime.UtcNow.ToString();
                
                // Set test value
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                };
                await _distributedCache.SetStringAsync(testKey, testValue, options, cancellationToken);
                
                // Get test value
                var retrievedValue = await _distributedCache.GetStringAsync(testKey, cancellationToken);
                
                // Clean up test value
                await _distributedCache.RemoveAsync(testKey, cancellationToken);

                if (retrievedValue == testValue)
                {
                    _logger.LogDebug("Distributed cache health check passed");
                    return HealthCheckResult.Healthy("Distributed cache is working correctly");
                }
                else
                {
                    _logger.LogWarning("Distributed cache health check failed: value mismatch");
                    return HealthCheckResult.Unhealthy("Distributed cache returned incorrect value");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Distributed cache health check failed with exception");
                return HealthCheckResult.Unhealthy("Distributed cache is not accessible", ex);
            }
        }
    }
} 