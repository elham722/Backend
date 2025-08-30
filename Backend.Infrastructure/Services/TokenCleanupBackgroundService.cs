using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Backend.Application.Common.Interfaces.Infrastructure;

namespace Backend.Infrastructure.Services
{
    /// <summary>
    /// Background service for cleaning up expired tokens from Redis cache
    /// </summary>
    public class TokenCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupBackgroundService> _logger;
        private readonly TokenCleanupOptions _options;

        public TokenCleanupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupBackgroundService> logger,
            IOptions<TokenCleanupOptions> options)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token cleanup background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokensAsync(stoppingToken);
                    
                    // Wait for the configured interval
                    await Task.Delay(_options.CleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service is stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during token cleanup");
                    
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Token cleanup background service stopped");
        }

        private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tokenCacheService = scope.ServiceProvider.GetRequiredService<ITokenCacheService>();

                _logger.LogDebug("Starting token cleanup process");
                
                await tokenCacheService.CleanupExpiredTokensAsync();
                
                _logger.LogDebug("Token cleanup process completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token cleanup process");
            }
        }
    }


} 