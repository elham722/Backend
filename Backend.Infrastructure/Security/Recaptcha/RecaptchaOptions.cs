namespace Backend.Infrastructure.Security.Recaptcha;

/// <summary>
/// reCAPTCHA version types
/// </summary>
public enum RecaptchaVersion 
{ 
    /// <summary>
    /// reCAPTCHA v2 with checkbox
    /// </summary>
    V2Checkbox, 
    
    /// <summary>
    /// reCAPTCHA v2 invisible
    /// </summary>
    V2Invisible, 
    
    /// <summary>
    /// reCAPTCHA v3
    /// </summary>
    V3 
}

/// <summary>
/// Configuration options for reCAPTCHA
/// </summary>
public sealed class RecaptchaOptions
{
    /// <summary>
    /// Whether reCAPTCHA is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// reCAPTCHA version to use
    /// </summary>
    public RecaptchaVersion Version { get; set; } = RecaptchaVersion.V2Invisible;
    
    /// <summary>
    /// Site key from Google reCAPTCHA
    /// </summary>
    public string SiteKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Secret key from Google reCAPTCHA
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Minimum score for v3 (0.0 = bot, 1.0 = human)
    /// </summary>
    public double MinimumScore { get; set; } = 0.5;
    
    /// <summary>
    /// Google verification URL
    /// </summary>
    public string VerifyUrl { get; set; } = "https://www.google.com/recaptcha/api/siteverify";
    
    /// <summary>
    /// Bypass verification in development environment
    /// </summary>
    public bool BypassInDevelopment { get; set; } = true;
} 