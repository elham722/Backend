namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Service for SMS operations
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send SMS verification code
    /// </summary>
    Task<bool> SendVerificationCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a random SMS verification code
    /// </summary>
    string GenerateVerificationCode(int length = 6);

    /// <summary>
    /// Validate phone number format
    /// </summary>
    bool IsValidPhoneNumber(string phoneNumber);

    /// <summary>
    /// Get SMS rate limit status for a phone number
    /// </summary>
    Task<bool> IsRateLimitedAsync(string phoneNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get remaining SMS quota for a phone number
    /// </summary>
    Task<int> GetRemainingQuotaAsync(string phoneNumber, CancellationToken cancellationToken = default);
} 