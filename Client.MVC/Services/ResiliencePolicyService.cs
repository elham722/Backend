using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using System.Net;
using System.Net.Sockets;

namespace Client.MVC.Services
{
    /// <summary>
    /// Configuration for resilience policies
    /// </summary>
    public class ResiliencePolicyConfig
    {
        public int RetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 2;
        public int CircuitBreakerThreshold { get; set; } = 3;
        public int CircuitBreakerDurationSeconds { get; set; } = 30;
        public int TimeoutSeconds { get; set; } = 10; // Reduced from 30 to 10 seconds
    }

    /// <summary>
    /// Service for creating and managing Polly resilience policies
    /// </summary>
    public class ResiliencePolicyService
    {
        private readonly ILogger<ResiliencePolicyService> _logger;
        private readonly ResiliencePolicyConfig _authConfig;
        private readonly ResiliencePolicyConfig _generalConfig;
        private readonly ResiliencePolicyConfig _readOnlyConfig;

        public ResiliencePolicyService(ILogger<ResiliencePolicyService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Load configuration
            _authConfig = LoadPolicyConfig(configuration, "ResiliencePolicies:Auth");
            _generalConfig = LoadPolicyConfig(configuration, "ResiliencePolicies:General");
            _readOnlyConfig = LoadPolicyConfig(configuration, "ResiliencePolicies:ReadOnly");
            
            _logger.LogInformation("ResiliencePolicyService initialized with Auth: {AuthRetryCount} retries, General: {GeneralRetryCount} retries", 
                _authConfig.RetryCount, _generalConfig.RetryCount);
        }

        private ResiliencePolicyConfig LoadPolicyConfig(IConfiguration configuration, string sectionName)
        {
            var section = configuration.GetSection(sectionName);
            return new ResiliencePolicyConfig
            {
                RetryCount = section.GetValue<int>("RetryCount", 3),
                RetryDelaySeconds = section.GetValue<int>("RetryDelaySeconds", 2),
                CircuitBreakerThreshold = section.GetValue<int>("CircuitBreakerThreshold", 3),
                CircuitBreakerDurationSeconds = section.GetValue<int>("CircuitBreakerDurationSeconds", 30),
                TimeoutSeconds = section.GetValue<int>("TimeoutSeconds", 30)
            };
        }

        /// <summary>
        /// Create retry policy for authentication operations with shorter backoff
        /// </summary>
        public AsyncRetryPolicy<HttpResponseMessage> CreateAuthRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: _authConfig.RetryCount,
                    sleepDurationProvider: retryAttempt => 
                        TimeSpan.FromSeconds(_authConfig.RetryDelaySeconds * retryAttempt), // Linear backoff for faster response
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogWarning(
                            "Auth retry {RetryAttempt} after {Delay}ms for {OperationKey}. " +
                            "Status: {StatusCode}, Exception: {Exception}",
                            retryAttempt, timespan.TotalMilliseconds, context.OperationKey,
                            outcome.Result?.StatusCode, outcome.Exception?.Message);
                    });
        }

        /// <summary>
        /// Create retry policy for general API operations
        /// </summary>
        public AsyncRetryPolicy<HttpResponseMessage> CreateGeneralRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: _generalConfig.RetryCount,
                    sleepDurationProvider: retryAttempt => 
                        TimeSpan.FromSeconds(_generalConfig.RetryDelaySeconds * Math.Pow(2, retryAttempt - 1)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogInformation(
                            "General retry {RetryAttempt} after {Delay}ms for {OperationKey}. " +
                            "Status: {StatusCode}",
                            retryAttempt, timespan.TotalMilliseconds, context.OperationKey,
                            outcome.Result?.StatusCode);
                    });
        }

        /// <summary>
        /// Create retry policy for read-only operations
        /// </summary>
        public AsyncRetryPolicy<HttpResponseMessage> CreateReadOnlyRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: _readOnlyConfig.RetryCount,
                    sleepDurationProvider: retryAttempt => 
                        TimeSpan.FromSeconds(_readOnlyConfig.RetryDelaySeconds * Math.Pow(2, retryAttempt - 1)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogInformation(
                            "Read-only retry {RetryAttempt} after {Delay}ms for {OperationKey}. " +
                            "Status: {StatusCode}",
                            retryAttempt, timespan.TotalMilliseconds, context.OperationKey,
                            outcome.Result?.StatusCode);
                    });
        }

        /// <summary>
        /// Create circuit breaker policy for authentication operations
        /// </summary>
        public AsyncCircuitBreakerPolicy<HttpResponseMessage> CreateAuthCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: _authConfig.CircuitBreakerThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(_authConfig.CircuitBreakerDurationSeconds),
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogError(
                            "Auth circuit breaker opened for {Duration}ms. " +
                            "Status: {StatusCode}, Exception: {Exception}",
                            timespan.TotalMilliseconds, outcome.Result?.StatusCode, outcome.Exception?.Message);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Auth circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Auth circuit breaker half-open");
                    });
        }

        /// <summary>
        /// Create circuit breaker policy for general API operations
        /// </summary>
        public AsyncCircuitBreakerPolicy<HttpResponseMessage> CreateGeneralCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: _generalConfig.CircuitBreakerThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(_generalConfig.CircuitBreakerDurationSeconds),
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogWarning(
                            "General circuit breaker opened for {Duration}ms. " +
                            "Status: {StatusCode}",
                            timespan.TotalMilliseconds, outcome.Result?.StatusCode);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("General circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("General circuit breaker half-open");
                    });
        }

        /// <summary>
        /// Create circuit breaker policy for read-only operations
        /// </summary>
        public AsyncCircuitBreakerPolicy<HttpResponseMessage> CreateReadOnlyCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: _readOnlyConfig.CircuitBreakerThreshold,
                    durationOfBreak: TimeSpan.FromSeconds(_readOnlyConfig.CircuitBreakerDurationSeconds),
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogWarning(
                            "Read-only circuit breaker opened for {Duration}ms. " +
                            "Status: {StatusCode}",
                            timespan.TotalMilliseconds, outcome.Result?.StatusCode);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Read-only circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Read-only circuit breaker half-open");
                    });
        }

        /// <summary>
        /// Create timeout policy
        /// </summary>
        public AsyncTimeoutPolicy<HttpResponseMessage> CreateTimeoutPolicy(int timeoutSeconds)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(
                timeout: TimeSpan.FromSeconds(timeoutSeconds),
                timeoutStrategy: TimeoutStrategy.Optimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    _logger.LogWarning(
                        "Request timeout after {Timeout}ms for {OperationKey}",
                        timespan.TotalMilliseconds, context.OperationKey);
                    return Task.CompletedTask;
                });
        }

        /// <summary>
        /// Create combined policy for authentication operations (most resilient)
        /// Order: Timeout -> Circuit Breaker -> Retry
        /// </summary>
        public IAsyncPolicy<HttpResponseMessage> CreateAuthPolicy()
        {
            return Policy.WrapAsync(
                CreateTimeoutPolicy(_authConfig.TimeoutSeconds),
                CreateAuthCircuitBreakerPolicy(),
                CreateAuthRetryPolicy()
            );
        }

        /// <summary>
        /// Create optimized policy for critical auth operations (login/refresh)
        /// with faster timeout and more aggressive circuit breaker
        /// </summary>
        public IAsyncPolicy<HttpResponseMessage> CreateCriticalAuthPolicy()
        {
            // Use even more aggressive settings for critical auth operations
            var criticalTimeoutPolicy = CreateTimeoutPolicy(5); // 5 seconds timeout
            var criticalCircuitBreaker = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 1, // Break after 1 failure
                    durationOfBreak: TimeSpan.FromSeconds(10), // Short break duration
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogError(
                            "Critical auth circuit breaker opened for {Duration}ms. " +
                            "Status: {StatusCode}, Exception: {Exception}",
                            timespan.TotalMilliseconds, outcome.Result?.StatusCode, outcome.Exception?.Message);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Critical auth circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Critical auth circuit breaker half-open");
                    });

            var criticalRetryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                .Or<SocketException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 1, // Only 1 retry for critical operations
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(0.5), // Very short delay
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogWarning(
                            "Critical auth retry {RetryAttempt} after {Delay}ms for {OperationKey}. " +
                            "Status: {StatusCode}, Exception: {Exception}",
                            retryAttempt, timespan.TotalMilliseconds, context.OperationKey,
                            outcome.Result?.StatusCode, outcome.Exception?.Message);
                    });

            return Policy.WrapAsync(criticalTimeoutPolicy, criticalCircuitBreaker, criticalRetryPolicy);
        }

        /// <summary>
        /// Create combined policy for general API operations
        /// Order: Timeout -> Circuit Breaker -> Retry
        /// </summary>
        public IAsyncPolicy<HttpResponseMessage> CreateGeneralPolicy()
        {
            return Policy.WrapAsync(
                CreateTimeoutPolicy(_generalConfig.TimeoutSeconds),
                CreateGeneralCircuitBreakerPolicy(),
                CreateGeneralRetryPolicy()
            );
        }

        /// <summary>
        /// Create policy for read-only operations (less aggressive but still with circuit breaker)
        /// Order: Timeout -> Circuit Breaker -> Retry
        /// </summary>
        public IAsyncPolicy<HttpResponseMessage> CreateReadOnlyPolicy()
        {
            return Policy.WrapAsync(
                CreateTimeoutPolicy(_readOnlyConfig.TimeoutSeconds),
                CreateReadOnlyCircuitBreakerPolicy(),
                CreateReadOnlyRetryPolicy()
            );
        }
    }
} 