namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Configuration settings for CAPTCHA service
/// </summary>
public class CaptchaSettings
{
    /// <summary>
    /// Whether CAPTCHA is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// CAPTCHA action name (e.g., "register", "login")
    /// </summary>
    public string Action { get; set; } = "default";

    /// <summary>
    /// Whether to enable CAPTCHA for all requests
    /// </summary>
    public bool RequireForAllRequests { get; set; } = false;

    /// <summary>
    /// IP addresses that are exempt from CAPTCHA
    /// </summary>
    public List<string> ExemptIpAddresses { get; set; } = new();

    /// <summary>
    /// Rate limiting settings for CAPTCHA
    /// </summary>
    public CaptchaRateLimitSettings RateLimit { get; set; } = new();

    /// <summary>
    /// Whether to log CAPTCHA validation attempts
    /// </summary>
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// Rate limiting settings for CAPTCHA
/// </summary>
public class CaptchaRateLimitSettings
{
    /// <summary>
    /// Maximum failed attempts per IP address
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Time window for rate limiting in minutes
    /// </summary>
    public int TimeWindowMinutes { get; set; } = 15;

    /// <summary>
    /// Whether to block IP after exceeding limit
    /// </summary>
    public bool BlockIpAfterLimit { get; set; } = true;

    /// <summary>
    /// Block duration in minutes
    /// </summary>
    public int BlockDurationMinutes { get; set; } = 60;
} 