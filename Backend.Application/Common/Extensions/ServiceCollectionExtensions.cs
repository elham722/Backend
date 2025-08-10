using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Common.Behaviors;
using Backend.Application.Common.Validation;

namespace Backend.Application.Common.Extensions;

/// <summary>
/// Extensions for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assembly">Assembly to scan</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly assembly)
    {
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register AutoMapper
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Register Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        // Register Memory Cache
        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Registers application services from the current assembly
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services.AddApplicationServices(Assembly.GetExecutingAssembly());
    }
} 