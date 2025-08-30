using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Backend.Identity.Services;

/// <summary>
/// Service for handling email confirmation functionality
/// </summary>
public interface IEmailConfirmationService
{
    /// <summary>
    /// Send email confirmation link to user
    /// </summary>
    Task<bool> SendEmailConfirmationAsync(ApplicationUser user, string callbackUrl);

    /// <summary>
    /// Confirm user's email address
    /// </summary>
    Task<bool> ConfirmEmailAsync(string userId, string token);

    /// <summary>
    /// Resend email confirmation
    /// </summary>
    Task<bool> ResendEmailConfirmationAsync(string email);

    /// <summary>
    /// Check if user's email is confirmed
    /// </summary>
    Task<bool> IsEmailConfirmedAsync(string userId);

    /// <summary>
    /// Get base URL from settings
    /// </summary>
    string GetBaseUrl();
}

/// <summary>
/// Implementation of email confirmation service
/// </summary>
public class EmailConfirmationService : IEmailConfirmationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailConfirmationService> _logger;
    private readonly EmailConfirmationSettings _settings;

    public EmailConfirmationService(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ILogger<EmailConfirmationService> logger,
        IOptions<EmailConfirmationSettings> settings)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Send email confirmation link to user
    /// </summary>
    public async Task<bool> SendEmailConfirmationAsync(ApplicationUser user, string callbackUrl)
    {
        try
        {
            if (user == null)
            {
                _logger.LogWarning("Attempted to send confirmation email to null user");
                return false;
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("User {UserId} has no email address", user.Id);
                return false;
            }

            // Generate confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Failed to generate email confirmation token for user {UserId}", user.Id);
                return false;
            }

            // Encode the callback URL
            var encodedCallbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

            // Create confirmation link
            var confirmationLink = $"{encodedCallbackUrl}?userId={user.Id}&token={token}";

            // Prepare email content
            var subject = _settings.EmailConfirmationSubject;
            var body = _settings.EmailConfirmationBodyTemplate
                .Replace("{UserName}", user.UserName ?? "User")
                .Replace("{ConfirmationLink}", confirmationLink)
                .Replace("{ExpiryHours}", _settings.TokenExpiryHours.ToString());

            // Send email
            var emailResult = await _emailSender.SendEmailAsync(
                user.Email,
                subject,
                body,
                isHtml: true);

            if (emailResult.IsSuccess) // ✅ Now using EmailResult
            {
                _logger.LogInformation("Email confirmation sent successfully to user {UserId} at {Email}", 
                    user.Id, user.Email);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send email confirmation to user {UserId}: {Error}", 
                    user.Id, emailResult.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email confirmation to user {UserId}", user?.Id);
            return false;
        }
    }

    /// <summary>
    /// Confirm user's email address
    /// </summary>
    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Invalid confirmation attempt: UserId or token is null/empty");
                return false;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation attempted for non-existent user: {UserId}", userId);
                return false;
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("User {UserId} email already confirmed", userId);
                return true;
            }

            // Confirm email
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmed successfully for user {UserId}", userId);
                return true;
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}", userId, errors);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Resend email confirmation
    /// </summary>
    public async Task<bool> ResendEmailConfirmationAsync(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Attempted to resend confirmation email with null/empty email");
                return false;
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Resend confirmation attempted for non-existent email: {Email}", email);
                return false;
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("User {UserId} email already confirmed, no need to resend", user.Id);
                return true;
            }

            // Generate new confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Failed to generate email confirmation token for user {UserId}", user.Id);
                return false;
            }

            // Create confirmation link using settings
            var confirmationLink = $"{_settings.BaseUrl}/confirm-email?userId={user.Id}&token={token}";

            // Prepare email content
            var subject = _settings.EmailConfirmationSubject;
            var body = _settings.EmailConfirmationBodyTemplate
                .Replace("{UserName}", user.UserName ?? "User")
                .Replace("{ConfirmationLink}", confirmationLink)
                .Replace("{ExpiryHours}", _settings.TokenExpiryHours.ToString());

            // Send email
            var emailResult = await _emailSender.SendEmailAsync(
                user.Email,
                subject,
                body,
                isHtml: true);

            if (emailResult.IsSuccess) // ✅ Now using EmailResult
            {
                _logger.LogInformation("Email confirmation resent successfully to user {UserId} at {Email}", 
                    user.Id, user.Email);
                return true;
            }
            else
            {
                _logger.LogError("Failed to resend email confirmation to user {UserId}: {Error}", 
                    user.Id, emailResult.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email confirmation for email {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Get base URL from settings
    /// </summary>
    public string GetBaseUrl()
    {
        return _settings.BaseUrl;
    }

    /// <summary>
    /// Check if user's email is confirmed
    /// </summary>
    public async Task<bool> IsEmailConfirmedAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var user = await _userManager.FindByIdAsync(userId);
            return user?.EmailConfirmed ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email confirmation status for user {UserId}", userId);
            return false;
        }
    }
}

/// <summary>
/// Configuration settings for email confirmation
/// </summary>
public class EmailConfirmationSettings
{
    /// <summary>
    /// Base URL for the application
    /// </summary>
    public string BaseUrl { get; set; } = "https://localhost:5001";

    /// <summary>
    /// Email confirmation subject
    /// </summary>
    public string EmailConfirmationSubject { get; set; } = "تأیید ایمیل حساب کاربری";

    /// <summary>
    /// Email confirmation body template
    /// </summary>
    public string EmailConfirmationBodyTemplate { get; set; } = @"
        <div dir='rtl' style='font-family: Tahoma, Arial, sans-serif;'>
            <h2>سلام {UserName} عزیز</h2>
            <p>برای تأیید ایمیل حساب کاربری خود، روی لینک زیر کلیک کنید:</p>
            <p><a href='{ConfirmationLink}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>تأیید ایمیل</a></p>
            <p>این لینک تا {ExpiryHours} ساعت معتبر است.</p>
            <p>اگر شما این درخواست را نکرده‌اید، این ایمیل را نادیده بگیرید.</p>
            <hr>
            <p style='font-size: 12px; color: #666;'>این ایمیل به صورت خودکار ارسال شده است.</p>
        </div>";

    /// <summary>
    /// Token expiry time in hours
    /// </summary>
    public int TokenExpiryHours { get; set; } = 24;

    /// <summary>
    /// Maximum retry attempts for sending confirmation emails
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay between retry attempts in seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Enable detailed logging for email confirmation
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
} 