using AutoMapper;

namespace Backend.Application.Mappers.Profiles;

/// <summary>
/// Base mapping profile with common mapping configurations
/// </summary>
public abstract class BaseMappingProfile : Profile
{
    protected BaseMappingProfile()
    {
        ConfigureEntityMappings();
        ConfigureDtoMappings();
    }

    /// <summary>
    /// Configure entity-specific mappings
    /// </summary>
    protected abstract void ConfigureEntityMappings();

    /// <summary>
    /// Configure DTO-specific mappings
    /// </summary>
    protected abstract void ConfigureDtoMappings();

    /// <summary>
    /// Helper method to ignore common properties that should not be mapped
    /// </summary>
    protected IMappingExpression<TSource, TDestination> IgnoreCommonProperties<TSource, TDestination>(
        IMappingExpression<TSource, TDestination> mapping)
    {
        return mapping;
    }
} 