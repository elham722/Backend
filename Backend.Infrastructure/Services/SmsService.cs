using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Implementation of SMS service with rate limiting and mock functionality
/// </summary>
public class SmsService : ISmsService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<SmsService> _logger;
    private const string RateLimitPrefix = "sms_rate_limit:";
    private const string QuotaPrefix = "sms_quota:";
    private const int MaxSmsPerHour = 5;
    private const int MaxSmsPerDay = 20;

    public SmsService(IDistributedCache cache, ILogger<SmsService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsValidPhoneNumber(phoneNumber))
            {
                _logger.LogWarning("Invalid phone number format: {PhoneNumber}", phoneNumber);
                return false;
            }

            // Check rate limits
            if (await IsRateLimitedAsync(phoneNumber, cancellationToken))
            {
                _logger.LogWarning("SMS rate limit exceeded for phone number: {PhoneNumber}", phoneNumber);
                return false;
            }

            // Check quota
            if (await GetRemainingQuotaAsync(phoneNumber, cancellationToken) <= 0)
            {
                _logger.LogWarning("SMS quota exceeded for phone number: {PhoneNumber}", phoneNumber);
                return false;
            }

            // In production, this would call a real SMS provider (Twilio, SendGrid, etc.)
            var success = await SendSmsViaProviderAsync(phoneNumber, code, cancellationToken);
            
            if (success)
            {
                // Update rate limiting and quota
                await UpdateRateLimitAsync(phoneNumber, cancellationToken);
                await UpdateQuotaAsync(phoneNumber, cancellationToken);
                
                _logger.LogInformation("SMS verification code sent successfully to {PhoneNumber}", phoneNumber);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS verification code to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public string GenerateVerificationCode(int length = 6)
    {
        if (length <= 0 || length > 10)
            throw new ArgumentException("Code length must be between 1 and 10", nameof(length));

        var random = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(random);

        var code = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            code.Append(random[i] % 10);
        }

        return code.ToString();
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Iranian mobile phone number format: +98 9XX XXX XXXX or 09XX XXX XXXX
        var pattern = @"^(\+98|0)?9\d{9}$";
        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, pattern);
    }

    public async Task<bool> IsRateLimitedAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var hourlyKey = $"{RateLimitPrefix}hourly:{phoneNumber}";
        var dailyKey = $"{RateLimitPrefix}daily:{phoneNumber}";

        var hourlyCount = await GetCacheValueAsync<int>(hourlyKey, cancellationToken);
        var dailyCount = await GetCacheValueAsync<int>(dailyKey, cancellationToken);

        return hourlyCount >= MaxSmsPerHour || dailyCount >= MaxSmsPerDay;
    }

    public async Task<int> GetRemainingQuotaAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var hourlyKey = $"{QuotaPrefix}hourly:{phoneNumber}";
        var dailyKey = $"{QuotaPrefix}daily:{phoneNumber}";

        var hourlyRemaining = Math.Max(0, MaxSmsPerHour - await GetCacheValueAsync<int>(hourlyKey, cancellationToken));
        var dailyRemaining = Math.Max(0, MaxSmsPerDay - await GetCacheValueAsync<int>(dailyKey, cancellationToken));

        return Math.Min(hourlyRemaining, dailyRemaining);
    }

    private async Task<bool> SendSmsViaProviderAsync(string phoneNumber, string code, CancellationToken cancellationToken)
    {
        try
        {
            // Mock SMS sending - in production, replace with real SMS provider
            var message = $"Your verification code is: {code}. Valid for 10 minutes.";
            
            // Simulate network delay
            await Task.Delay(100, cancellationToken);
            
            // Simulate 95% success rate
            var random = new Random();
            var success = random.Next(100) < 95;
            
            if (success)
            {
                _logger.LogInformation("Mock SMS sent to {PhoneNumber}: {Message}", phoneNumber, message);
            }
            else
            {
                _logger.LogWarning("Mock SMS failed to {PhoneNumber}", phoneNumber);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in mock SMS provider for {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    private async Task UpdateRateLimitAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        var hourlyKey = $"{RateLimitPrefix}hourly:{phoneNumber}";
        var dailyKey = $"{RateLimitPrefix}daily:{phoneNumber}";

        // Increment hourly count
        var hourlyCount = await GetCacheValueAsync<int>(hourlyKey, cancellationToken);
        await SetCacheValueAsync(hourlyKey, hourlyCount + 1, TimeSpan.FromHours(1), cancellationToken);

        // Increment daily count
        var dailyCount = await GetCacheValueAsync<int>(dailyKey, cancellationToken);
        await SetCacheValueAsync(dailyKey, dailyCount + 1, TimeSpan.FromDays(1), cancellationToken);
    }

    private async Task UpdateQuotaAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        var hourlyKey = $"{QuotaPrefix}hourly:{phoneNumber}";
        var dailyKey = $"{QuotaPrefix}daily:{phoneNumber}";

        // Update hourly quota
        var hourlyCount = await GetCacheValueAsync<int>(hourlyKey, cancellationToken);
        await SetCacheValueAsync(hourlyKey, hourlyCount + 1, TimeSpan.FromHours(1), cancellationToken);

        // Update daily quota
        var dailyCount = await GetCacheValueAsync<int>(dailyKey, cancellationToken);
        await SetCacheValueAsync(dailyKey, dailyCount + 1, TimeSpan.FromDays(1), cancellationToken);
    }

    private async Task<T> GetCacheValueAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(value))
                return default(T);

            return JsonSerializer.Deserialize<T>(value) ?? default(T);
        }
        catch
        {
            return default(T);
        }
    }

    private async Task SetCacheValueAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken)
    {
        try
        {
            var jsonValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            
            await _cache.SetStringAsync(key, jsonValue, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }
} 