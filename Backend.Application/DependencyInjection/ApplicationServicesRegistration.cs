using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Backend.Application.Common.Extensions;
using Backend.Application.Common.Interfaces;
using Backend.Application.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backend.Application.DependencyInjection;

/// <summary>
/// Registration of application services
/// </summary>
public static class ApplicationServicesRegistration
{
    /// <summary>
    /// Registers all application services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddApplicationServices();

        // Register application interfaces
        services.AddScoped<IDateTimeService, DateTimeService>();

        // Register HttpContextAccessor for CurrentUserService
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        return services;
    }
} 