namespace Backend.Application.Common.Security;

/// <summary>
/// Service for human verification (CAPTCHA) operations
/// </summary>
public interface IHumanVerificationService
{
    /// <summary>
    /// Verify CAPTCHA token
    /// </summary>
    /// <param name="token">CAPTCHA token from client</param>
    /// <param name="action">Action being performed (e.g., "register", "login")</param>
    /// <param name="userIp">User's IP address</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Verification result</returns>
    Task<CaptchaVerificationResult> VerifyAsync(
        string token, 
        string? action = null, 
        string? userIp = null, 
        CancellationToken ct = default);
}

/// <summary>
/// Result of CAPTCHA verification
/// </summary>
public sealed record CaptchaVerificationResult(
    bool Success,
    double? Score = null,
    string? Action = null,
    string? Hostname = null,
    string[]? ErrorCodes = null
); 