using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Backend.Application.Common.Extensions;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Infrastructure;
using Backend.Application.Common.Interfaces.Infrastructure;
using Backend.Application.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mapster;
using MapsterMapper;
using System.Reflection;
using FluentValidation;

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
        // Register application services
        services.AddApplicationServices();

        // Register application interfaces
        services.AddScoped<IDateTimeService, DateTimeService>();

        // Register dispatchers
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        // Register HttpContextAccessor for CurrentUserService
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // Register Mapster
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
} 