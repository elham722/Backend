namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Service for CAPTCHA validation
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// Generate a new CAPTCHA challenge
    /// </summary>
    /// <param name="ipAddress">Client IP address</param>
    /// <returns>CAPTCHA challenge</returns>
    Task<CaptchaChallenge> GenerateChallengeAsync(string? ipAddress = null);

    /// <summary>
    /// Validate CAPTCHA answer
    /// </summary>
    /// <param name="challengeId">CAPTCHA challenge ID</param>
    /// <param name="answer">CAPTCHA answer from client</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <returns>Validation result with score</returns>
    Task<CaptchaValidationResult> ValidateAsync(string challengeId, string answer, string? ipAddress = null);

    /// <summary>
    /// Check if CAPTCHA is required for the request
    /// </summary>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="action">Action being performed (e.g., "register", "login")</param>
    /// <returns>Whether CAPTCHA is required</returns>
    Task<bool> IsRequiredAsync(string? ipAddress = null, string action = "default");

    /// <summary>
    /// Get CAPTCHA configuration for client
    /// </summary>
    /// <returns>CAPTCHA configuration</returns>
    CaptchaConfiguration GetConfiguration();
}

/// <summary>
/// CAPTCHA validation result
/// </summary>
public class CaptchaValidationResult
{
    public bool IsValid { get; set; }
    public double Score { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ChallengePassed { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// CAPTCHA configuration for client
/// </summary>
public class CaptchaConfiguration
{
    public string SiteKey { get; set; } = "simple-captcha";
    public string Action { get; set; } = "captcha";
    public double Threshold { get; set; } = 1.0;
    public bool IsEnabled { get; set; } = true;
    public CaptchaType Type { get; set; } = CaptchaType.Simple;
}

/// <summary>
/// CAPTCHA challenge model
/// </summary>
public class CaptchaChallenge
{
    public string Id { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// CAPTCHA type enumeration
/// </summary>
public enum CaptchaType
{
    Simple,
    GoogleReCaptcha
}

/// <summary>
/// Google reCAPTCHA validation request
/// </summary>
public class GoogleReCaptchaValidationRequest
{
    public string Token { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}

/// <summary>
/// Google reCAPTCHA validation response
/// </summary>
public class GoogleReCaptchaValidationResponse
{
    public bool Success { get; set; }
    public double Score { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime ChallengeTime { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public List<string> ErrorCodes { get; set; } = new();
} 