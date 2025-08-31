namespace Backend.Application.Common.Security;

/// <summary>
/// Marker interface for requests that require CAPTCHA verification
/// </summary>
public interface IRequireCaptcha
{
    /// <summary>
    /// CAPTCHA token from client
    /// </summary>
    string CaptchaToken { get; }
    
    /// <summary>
    /// CAPTCHA action (optional for v3 or invisible)
    /// </summary>
    string? CaptchaAction => null;
} 