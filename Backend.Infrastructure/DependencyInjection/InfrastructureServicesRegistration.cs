using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.DependencyInjection;
using Backend.Infrastructure.Cache;
using Backend.Infrastructure.Email;
using Backend.Infrastructure.FileStorage;
using Backend.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering infrastructure services
/// </summary>
public static class InfrastructureServicesRegistration
{
    /// <summary>
    /// Registers all infrastructure layer services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection ConfigureInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Application layer services
        services.ConfigureApplicationServices();

        // Register Infrastructure services
        services.AddInfrastructureCoreServices(configuration);
        services.AddEmailServices(configuration);
        services.AddCacheServices(configuration);
        services.AddFileStorageServices(configuration);

        return services;
    }

    /// <summary>
    /// Registers core infrastructure services
    /// </summary>
    private static IServiceCollection AddInfrastructureCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register date time service
        services.AddScoped<IDateTimeService, DateTimeService>();

        return services;
    }

    /// <summary>
    /// Registers email services
    /// </summary>
    private static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure email settings
        services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));

        // Register email senders based on configuration
        var emailProvider = configuration["EmailSettings:Provider"]?.ToLowerInvariant();

        switch (emailProvider)
        {
            case "sendgrid":
                services.AddScoped<IEmailSender, SendGridEmailSender>();
                break;
            case "smtp":
                services.AddScoped<IEmailSender, SmtpEmailSender>();
                break;
            default:
                // Default to SendGrid
                services.AddScoped<IEmailSender, SendGridEmailSender>();
                break;
        }

        // Register email template service
        services.AddScoped<EmailTemplateService>();

        return services;
    }

    /// <summary>
    /// Registers cache services
    /// </summary>
    private static IServiceCollection AddCacheServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure memory cache options
        services.Configure<MemoryCacheOptions>(
            configuration.GetSection("MemoryCache"));

        // Register cache service
        services.AddScoped<ICacheService, MemoryCacheService>();

        return services;
    }

    /// <summary>
    /// Registers file storage services
    /// </summary>
    private static IServiceCollection AddFileStorageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure local file storage options
        services.Configure<LocalFileStorageOptions>(
            configuration.GetSection("LocalFileStorage"));

        // Register file storage service
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }



    /// <summary>
    /// Registers infrastructure services with custom options
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        Action<InfrastructureOptions> configureOptions)
    {
        var options = new InfrastructureOptions();
        configureOptions(options);

        // Register with custom options
        services.Configure<EmailSettings>(opt =>
        {
            opt.Provider = options.EmailProvider;
            opt.ApiKey = options.EmailApiKey;
            opt.FromAddress = options.EmailFromAddress;
            opt.FromName = options.EmailFromName;
        });

        services.Configure<LocalFileStorageOptions>(opt =>
        {
            opt.BasePath = options.FileStorageBasePath;
            opt.MaxFileSizeBytes = options.MaxFileSizeBytes;
        });

        services.Configure<MemoryCacheOptions>(opt =>
        {
            opt.DefaultExpirationMinutes = options.CacheExpirationMinutes;
            opt.SizeLimit = options.CacheSizeLimit;
        });

        // Register services
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<ICacheService, MemoryCacheService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Register email sender based on provider
        switch (options.EmailProvider?.ToLowerInvariant())
        {
            case "sendgrid":
                services.AddScoped<IEmailSender, SendGridEmailSender>();
                break;
            case "smtp":
                services.AddScoped<IEmailSender, SmtpEmailSender>();
                break;
            default:
                services.AddScoped<IEmailSender, SendGridEmailSender>();
                break;
        }

        services.AddScoped<EmailTemplateService>();

        return services;
    }
}

/// <summary>
/// Infrastructure options for custom configuration
/// </summary>
public class InfrastructureOptions
{
    public string? EmailProvider { get; set; } = "SendGrid";
    public string? EmailApiKey { get; set; }
    public string? EmailFromAddress { get; set; }
    public string? EmailFromName { get; set; }
    public string FileStorageBasePath { get; set; } = "wwwroot/uploads";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
    public int CacheExpirationMinutes { get; set; } = 60;
    public int CacheSizeLimit { get; set; } = 1000;
} 