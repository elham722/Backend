using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Infrastructure;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using FluentValidation;
using Backend.Application.Common.Behaviors;
using MediatR;

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
    public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register AutoMapper with profiles from Application assembly
        services.AddAutoMapper(cfg => 
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
            // Add any additional configuration here if needed
        });

        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CaptchaBehavior<,>));

        // Register Memory Cache
        services.AddMemoryCache();


        // Register application interfaces
        services.AddScoped<IDateTimeService, DateTimeService>();
        
        // Register dispatchers
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        // Register HttpContextAccessor for CurrentUserService
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // Mapster is not needed - we use AutoMapper instead

        return services;
    }
} 