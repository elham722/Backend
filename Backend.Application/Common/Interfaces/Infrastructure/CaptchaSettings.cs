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
    /// CAPTCHA type to use
    /// </summary>
    public CaptchaType Type { get; set; } = CaptchaType.Simple;

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

    /// <summary>
    /// Google reCAPTCHA specific settings
    /// </summary>
    public GoogleReCaptchaSettings GoogleReCaptcha { get; set; } = new();
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

/// <summary>
/// Google reCAPTCHA specific configuration
/// </summary>
public class GoogleReCaptchaSettings
{
    /// <summary>
    /// Whether Google reCAPTCHA is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Google reCAPTCHA site key (public key for client)
    /// </summary>
    public string SiteKey { get; set; } = string.Empty;

    /// <summary>
    /// Google reCAPTCHA secret key (private key for server)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Minimum score threshold for reCAPTCHA v3 (0.0 to 1.0)
    /// </summary>
    public double ScoreThreshold { get; set; } = 0.5;

    /// <summary>
    /// Google reCAPTCHA API endpoint
    /// </summary>
    public string ApiEndpoint { get; set; } = "https://www.google.com/recaptcha/api/siteverify";

    /// <summary>
    /// Timeout for API calls in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Whether to use proxy for API calls
    /// </summary>
    public bool UseProxy { get; set; } = false;

    /// <summary>
    /// Proxy settings if UseProxy is true
    /// </summary>
    public ProxySettings? Proxy { get; set; }
}

/// <summary>
/// Proxy configuration for reCAPTCHA API calls
/// </summary>
public class ProxySettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 8080;
    public string? Username { get; set; }
    public string? Password { get; set; }
} 