using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Google reCAPTCHA service implementation
/// </summary>
public class GoogleReCaptchaService : ICaptchaService
{
    private readonly ILogger<GoogleReCaptchaService> _logger;
    private readonly IOptions<CaptchaSettings> _settings;
    private readonly IDistributedCache _cache;
    private readonly HttpClient _httpClient;

    public GoogleReCaptchaService(
        ILogger<GoogleReCaptchaService> logger,
        IOptions<CaptchaSettings> settings,
        IDistributedCache cache,
        HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Generate a new CAPTCHA challenge (not applicable for reCAPTCHA)
    /// </summary>
    public async Task<CaptchaChallenge> GenerateChallengeAsync(string? ipAddress = null)
    {
        // For Google reCAPTCHA, we don't generate challenges on the server
        // The challenge is generated on the client side
        var challengeId = GenerateChallengeId();
        var challenge = new CaptchaChallenge
        {
            Id = challengeId,
            Question = "Google reCAPTCHA challenge",
            Answer = "reCAPTCHA", // This will be replaced by the actual token
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Store challenge in cache for validation
        var cacheKey = $"recaptcha_challenge:{challengeId}";
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(challenge), options);

        _logger.LogDebug("Generated reCAPTCHA challenge {ChallengeId} for IP {IpAddress}", challengeId, ipAddress);

        return challenge;
    }

    /// <summary>
    /// Validate reCAPTCHA token
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
                    Action = "recaptcha",
                    ChallengePassed = "INVALID_INPUT",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = "Challenge ID and token are required"
                };
            }

            // Check rate limiting
            if (await IsRateLimitedAsync(ipAddress))
            {
                _logger.LogWarning("IP address {IpAddress} is rate limited for reCAPTCHA", ipAddress);
                return new CaptchaValidationResult
                {
                    IsValid = false,
                    Score = 0.0,
                    Action = "recaptcha",
                    ChallengePassed = "RATE_LIMITED",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = "Too many reCAPTCHA attempts, please try again later"
                };
            }

            // Validate the reCAPTCHA token with Google
            var validationResult = await ValidateWithGoogleAsync(answer, ipAddress);

            if (validationResult.Success)
            {
                // Check if score meets threshold
                var meetsThreshold = validationResult.Score >= _settings.Value.GoogleReCaptcha.ScoreThreshold;

                // Update rate limiting
                await UpdateRateLimitAsync(ipAddress, meetsThreshold);

                // Log the attempt
                if (_settings.Value.EnableLogging)
                {
                    _logger.LogInformation(
                        "reCAPTCHA validation for IP {IpAddress}: Score={Score}, Threshold={Threshold}, MeetsThreshold={MeetsThreshold}",
                        ipAddress, validationResult.Score, _settings.Value.GoogleReCaptcha.ScoreThreshold, meetsThreshold);
                }

                return new CaptchaValidationResult
                {
                    IsValid = meetsThreshold,
                    Score = validationResult.Score,
                    Action = validationResult.Action,
                    ChallengePassed = meetsThreshold ? "PASSED" : "SCORE_TOO_LOW",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = meetsThreshold ? null : $"Score {validationResult.Score} below threshold {_settings.Value.GoogleReCaptcha.ScoreThreshold}"
                };
            }
            else
            {
                // Update rate limiting
                await UpdateRateLimitAsync(ipAddress, false);

                var errorMessage = string.Join(", ", validationResult.ErrorCodes);
                _logger.LogWarning("reCAPTCHA validation failed for IP {IpAddress}: {ErrorCodes}", ipAddress, errorMessage);

                return new CaptchaValidationResult
                {
                    IsValid = false,
                    Score = 0.0,
                    Action = "recaptcha",
                    ChallengePassed = "VALIDATION_FAILED",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = $"reCAPTCHA validation failed: {errorMessage}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating reCAPTCHA for IP {IpAddress}", ipAddress);
            return new CaptchaValidationResult
            {
                IsValid = false,
                Score = 0.0,
                Action = "recaptcha",
                ChallengePassed = "VALIDATION_ERROR",
                Timestamp = DateTime.UtcNow,
                ErrorMessage = "reCAPTCHA validation failed, please try again"
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
            if (!_settings.Value.IsEnabled || !_settings.Value.GoogleReCaptcha.IsEnabled)
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
            _logger.LogError(ex, "Error checking if reCAPTCHA is required for IP {IpAddress}", ipAddress);
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
            SiteKey = _settings.Value.GoogleReCaptcha.SiteKey,
            Action = _settings.Value.GoogleReCaptcha.IsEnabled ? "recaptcha" : "disabled",
            Threshold = _settings.Value.GoogleReCaptcha.ScoreThreshold,
            IsEnabled = _settings.Value.IsEnabled && _settings.Value.GoogleReCaptcha.IsEnabled,
            Type = CaptchaType.GoogleReCaptcha
        };
    }

    /// <summary>
    /// Validate reCAPTCHA token with Google API
    /// </summary>
    private async Task<GoogleReCaptchaValidationResponse> ValidateWithGoogleAsync(string token, string? ipAddress)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _settings.Value.GoogleReCaptcha.SecretKey),
                new KeyValuePair<string, string>("response", token),
                new KeyValuePair<string, string>("remoteip", ipAddress ?? "")
            });

            var timeout = TimeSpan.FromSeconds(_settings.Value.GoogleReCaptcha.TimeoutSeconds);
            using var cts = new CancellationTokenSource(timeout);

            var response = await _httpClient.PostAsync(_settings.Value.GoogleReCaptcha.ApiEndpoint, content, cts.Token);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Google reCAPTCHA API returned error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return new GoogleReCaptchaValidationResponse
                {
                    Success = false,
                    ErrorCodes = new List<string> { $"HTTP_{response.StatusCode}" }
                };
            }

            var result = JsonSerializer.Deserialize<GoogleReCaptchaValidationResponse>(responseContent);
            return result ?? new GoogleReCaptchaValidationResponse
            {
                Success = false,
                ErrorCodes = new List<string> { "DESERIALIZATION_ERROR" }
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Google reCAPTCHA API call timed out");
            return new GoogleReCaptchaValidationResponse
            {
                Success = false,
                ErrorCodes = new List<string> { "TIMEOUT" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Google reCAPTCHA API");
            return new GoogleReCaptchaValidationResponse
            {
                Success = false,
                ErrorCodes = new List<string> { "API_ERROR" }
            };
        }
    }

    /// <summary>
    /// Generate unique challenge ID
    /// </summary>
    private string GenerateChallengeId()
    {
        var randomBytes = new byte[16];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
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

        var cacheKey = $"recaptcha_rate_limit:{ipAddress}";
        var rateLimitData = await _cache.GetStringAsync(cacheKey);
        
        if (string.IsNullOrEmpty(rateLimitData))
            return false;

        var rateLimit = JsonSerializer.Deserialize<RateLimitData>(rateLimitData);
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
            
            var updatedData = JsonSerializer.Serialize(rateLimit);
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

        var cacheKey = $"recaptcha_rate_limit:{ipAddress}";
        var rateLimitData = await _cache.GetStringAsync(cacheKey);
        
        var rateLimit = !string.IsNullOrEmpty(rateLimitData) 
            ? JsonSerializer.Deserialize<RateLimitData>(rateLimitData) 
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

        var updatedData = JsonSerializer.Serialize(rateLimit);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.Value.RateLimit.TimeWindowMinutes)
        };
        
        await _cache.SetStringAsync(cacheKey, updatedData, options);
    }
}

 