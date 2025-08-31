using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Factory for creating CAPTCHA services based on configuration
/// </summary>
public interface ICaptchaServiceFactory
{
    /// <summary>
    /// Create the appropriate CAPTCHA service based on configuration
    /// </summary>
    /// <returns>CAPTCHA service implementation</returns>
    ICaptchaService CreateCaptchaService();
}

/// <summary>
/// Factory implementation for creating CAPTCHA services
/// </summary>
public class CaptchaServiceFactory : ICaptchaServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<CaptchaSettings> _settings;
    private readonly ILogger<CaptchaServiceFactory> _logger;

    public CaptchaServiceFactory(
        IServiceProvider serviceProvider,
        IOptions<CaptchaSettings> settings,
        ILogger<CaptchaServiceFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create the appropriate CAPTCHA service based on configuration
    /// </summary>
    public ICaptchaService CreateCaptchaService()
    {
        try
        {
            var captchaType = _settings.Value.Type;
            var isGoogleEnabled = _settings.Value.GoogleReCaptcha.IsEnabled;

            // If Google reCAPTCHA is configured and enabled, use it
            if (captchaType == CaptchaType.GoogleReCaptcha && isGoogleEnabled)
            {
                if (string.IsNullOrEmpty(_settings.Value.GoogleReCaptcha.SiteKey) || 
                    string.IsNullOrEmpty(_settings.Value.GoogleReCaptcha.SecretKey))
                {
                    _logger.LogWarning("Google reCAPTCHA is enabled but SiteKey or SecretKey is missing. Falling back to Simple CAPTCHA.");
                    return CreateSimpleCaptchaService();
                }

                _logger.LogInformation("Creating Google reCAPTCHA service");
                return CreateGoogleReCaptchaService();
            }

            // Default to Simple CAPTCHA
            _logger.LogInformation("Creating Simple CAPTCHA service");
            return CreateSimpleCaptchaService();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating CAPTCHA service. Falling back to Simple CAPTCHA.");
            return CreateSimpleCaptchaService();
        }
    }

    /// <summary>
    /// Create Simple CAPTCHA service
    /// </summary>
    private ICaptchaService CreateSimpleCaptchaService()
    {
        return _serviceProvider.GetRequiredService<SimpleCaptchaService>();
    }

    /// <summary>
    /// Create Google reCAPTCHA service
    /// </summary>
    private ICaptchaService CreateGoogleReCaptchaService()
    {
        return _serviceProvider.GetRequiredService<GoogleReCaptchaService>();
    }
} 