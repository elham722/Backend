using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Simple CAPTCHA service that generates mathematical challenges
/// </summary>
public class SimpleCaptchaService : ICaptchaService
{
    private readonly ILogger<SimpleCaptchaService> _logger;
    private readonly IOptions<CaptchaSettings> _settings;
    private readonly IDistributedCache _cache;

    public SimpleCaptchaService(
        ILogger<SimpleCaptchaService> logger,
        IOptions<CaptchaSettings> settings,
        IDistributedCache cache)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Generate a new CAPTCHA challenge
    /// </summary>
    public async Task<CaptchaChallenge> GenerateChallengeAsync(string? ipAddress = null)
    {
        try
        {
            var random = new Random();
            var num1 = random.Next(1, 20);
            var num2 = random.Next(1, 20);
            var operation = random.Next(0, 2); // 0: addition, 1: multiplication

            string question;
            int answer;

            if (operation == 0)
            {
                question = $"{num1} + {num2} = ?";
                answer = num1 + num2;
            }
            else
            {
                question = $"{num1} × {num2} = ?";
                answer = num1 * num2;
            }

            var challengeId = GenerateChallengeId();
            var challenge = new CaptchaChallenge
            {
                Id = challengeId,
                Question = question,
                Answer = answer.ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5) // 5 minutes expiry
            };

            // Store challenge in cache
            var cacheKey = $"captcha_challenge:{challengeId}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(challenge), options);

            _logger.LogDebug("Generated CAPTCHA challenge {ChallengeId} for IP {IpAddress}", challengeId, ipAddress);

            return challenge;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CAPTCHA challenge for IP {IpAddress}", ipAddress);
            throw;
        }
    }

    /// <summary>
    /// Validate CAPTCHA answer
    /// </summary>
    public async Task<CaptchaValidationResult> ValidateAsync(string challengeId, string answer, string? ipAddress = null)
    {
        try
        {
            if (string.IsNullOrEmpty(challengeId) || string.IsNullOrEmpty(answer))
            {
                return new CaptchaValidationResult
                {
                    IsValid = false,
                    Score = 0.0,
                    Action = "captcha",
                    ChallengePassed = "INVALID_INPUT",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = "Challenge ID and answer are required"
                };
            }

            // Check rate limiting
            if (await IsRateLimitedAsync(ipAddress))
            {
                _logger.LogWarning("IP address {IpAddress} is rate limited for CAPTCHA", ipAddress);
                return new CaptchaValidationResult
                {
                    IsValid = false,
                    Score = 0.0,
                    Action = "captcha",
                    ChallengePassed = "RATE_LIMITED",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = "Too many CAPTCHA attempts, please try again later"
                };
            }

            // Get challenge from cache
            var cacheKey = $"captcha_challenge:{challengeId}";
            var challengeData = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(challengeData))
            {
                return new CaptchaValidationResult
                {
                    IsValid = false,
                    Score = 0.0,
                    Action = "captcha",
                    ChallengePassed = "CHALLENGE_EXPIRED",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = "CAPTCHA challenge expired or not found"
                };
            }

            var challenge = System.Text.Json.JsonSerializer.Deserialize<CaptchaChallenge>(challengeData);
            if (challenge == null)
            {
                return new CaptchaValidationResult
                {
                    IsValid = false,
                    Score = 0.0,
                    Action = "captcha",
                    ChallengePassed = "INVALID_CHALLENGE",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = "Invalid CAPTCHA challenge"
                };
            }

            // Check if challenge is expired
            if (DateTime.UtcNow > challenge.ExpiresAt)
            {
                // Remove expired challenge
                await _cache.RemoveAsync(cacheKey);
                
                return new CaptchaValidationResult
                {
                    IsValid = false,
                    Score = 0.0,
                    Action = "captcha",
                    ChallengePassed = "CHALLENGE_EXPIRED",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = "CAPTCHA challenge expired"
                };
            }

            // Validate answer
            var isCorrect = answer.Trim().Equals(challenge.Answer, StringComparison.OrdinalIgnoreCase);
            
            // Remove challenge from cache (one-time use)
            await _cache.RemoveAsync(cacheKey);

            // Update rate limiting
            await UpdateRateLimitAsync(ipAddress, isCorrect);

            // Log the attempt
            if (_settings.Value.EnableLogging)
            {
                _logger.LogInformation(
                    "CAPTCHA validation for IP {IpAddress}: Challenge={ChallengeId}, Correct={IsCorrect}",
                    ipAddress, challengeId, isCorrect);
            }

            return new CaptchaValidationResult
            {
                IsValid = isCorrect,
                Score = isCorrect ? 1.0 : 0.0,
                Action = "captcha",
                ChallengePassed = isCorrect ? "PASSED" : "FAILED",
                Timestamp = DateTime.UtcNow,
                ErrorMessage = isCorrect ? null : "Incorrect answer"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CAPTCHA for IP {IpAddress}", ipAddress);
            return new CaptchaValidationResult
            {
                IsValid = false,
                Score = 0.0,
                Action = "captcha",
                ChallengePassed = "VALIDATION_ERROR",
                Timestamp = DateTime.UtcNow,
                ErrorMessage = "CAPTCHA validation failed, please try again"
            };
        }
    }

    /// <summary>
    /// Check if CAPTCHA is required for the request
    /// </summary>
    public async Task<bool> IsRequiredAsync(string? ipAddress = null, string action = "default")
    {
        try
        {
            if (!_settings.Value.IsEnabled)
                return false;

            if (_settings.Value.RequireForAllRequests)
                return true;

            // Check if IP is exempt
            if (!string.IsNullOrEmpty(ipAddress) && _settings.Value.ExemptIpAddresses.Contains(ipAddress))
                return false;

            // Check if IP is blocked
            if (await IsRateLimitedAsync(ipAddress))
                return true;

            // Check if action requires CAPTCHA
            return action.Equals("register", StringComparison.OrdinalIgnoreCase) ||
                   action.Equals("login", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if CAPTCHA is required for IP {IpAddress}", ipAddress);
            return true; // Default to requiring CAPTCHA on error
        }
    }

    /// <summary>
    /// Get CAPTCHA configuration for client
    /// </summary>
    public CaptchaConfiguration GetConfiguration()
    {
        return new CaptchaConfiguration
        {
            SiteKey = "simple-captcha",
            Action = "captcha",
            Threshold = 1.0, // Must be exactly correct
            IsEnabled = _settings.Value.IsEnabled
        };
    }

    /// <summary>
    /// Generate unique challenge ID
    /// </summary>
    private string GenerateChallengeId()
    {
        var randomBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 16);
    }

    /// <summary>
    /// Check if IP is rate limited
    /// </summary>
    private async Task<bool> IsRateLimitedAsync(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return false;

        var cacheKey = $"captcha_rate_limit:{ipAddress}";
        var rateLimitData = await _cache.GetStringAsync(cacheKey);
        
        if (string.IsNullOrEmpty(rateLimitData))
            return false;

        var rateLimit = System.Text.Json.JsonSerializer.Deserialize<RateLimitData>(rateLimitData);
        if (rateLimit == null)
            return false;

        // Check if IP is blocked
        if (rateLimit.IsBlocked && rateLimit.BlockedUntil > DateTime.UtcNow)
            return true;

        // Check if too many failed attempts
        if (rateLimit.FailedAttempts >= _settings.Value.RateLimit.MaxFailedAttempts)
        {
            // Block the IP
            rateLimit.IsBlocked = true;
            rateLimit.BlockedUntil = DateTime.UtcNow.AddMinutes(_settings.Value.RateLimit.BlockDurationMinutes);
            
            var updatedData = System.Text.Json.JsonSerializer.Serialize(rateLimit);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.Value.RateLimit.BlockDurationMinutes)
            };
            
            await _cache.SetStringAsync(cacheKey, updatedData, options);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Update rate limiting for IP
    /// </summary>
    private async Task UpdateRateLimitAsync(string? ipAddress, bool wasSuccessful)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return;

        var cacheKey = $"captcha_rate_limit:{ipAddress}";
        var rateLimitData = await _cache.GetStringAsync(cacheKey);
        
        var rateLimit = !string.IsNullOrEmpty(rateLimitData) 
            ? System.Text.Json.JsonSerializer.Deserialize<RateLimitData>(rateLimitData) 
            : new RateLimitData();

        if (rateLimit == null)
            rateLimit = new RateLimitData();

        if (wasSuccessful)
        {
            rateLimit.SuccessfulAttempts++;
            rateLimit.FailedAttempts = Math.Max(0, rateLimit.FailedAttempts - 1); // Reduce failed attempts on success
        }
        else
        {
            rateLimit.FailedAttempts++;
        }

        rateLimit.LastAttempt = DateTime.UtcNow;

        var updatedData = System.Text.Json.JsonSerializer.Serialize(rateLimit);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.Value.RateLimit.TimeWindowMinutes)
        };
        
        await _cache.SetStringAsync(cacheKey, updatedData, options);
    }
}

/// <summary>
/// Rate limiting data for CAPTCHA
/// </summary>
public class RateLimitData
{
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime LastAttempt { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime BlockedUntil { get; set; }
} 