namespace Backend.Infrastructure.Email;

/// <summary>
/// Email settings configuration
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Email provider (SendGrid, SMTP, etc.)
    /// </summary>
    public string Provider { get; set; } = "SendGrid";

    /// <summary>
    /// API key for SendGrid
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// From email address
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// From name
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server address
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP username
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Enable SSL for SMTP
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Timeout for email operations in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts for failed emails
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Enable detailed error logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Validate email settings
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(FromAddress) || string.IsNullOrEmpty(FromName))
            return false;

        if (Provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
        {
            return !string.IsNullOrEmpty(ApiKey);
        }
        else if (Provider.Equals("SMTP", StringComparison.OrdinalIgnoreCase))
        {
            return !string.IsNullOrEmpty(SmtpServer) && 
                   !string.IsNullOrEmpty(SmtpUsername) && 
                   !string.IsNullOrEmpty(SmtpPassword) &&
                   SmtpPort > 0;
        }

        return false;
    }
}