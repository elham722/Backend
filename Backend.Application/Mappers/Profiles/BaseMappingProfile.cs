using AutoMapper;

namespace Backend.Application.Mappers.Profiles;

/// <summary>
/// Base mapping profile with common mapping configurations
/// </summary>
public abstract class BaseMappingProfile : Profile
{
    protected BaseMappingProfile()
    {
        // Common mapping configurations can be added here
        ConfigureCommonMappings();
    }

    /// <summary>
    /// Configure common mappings that apply to all entities
    /// </summary>
    protected virtual void ConfigureCommonMappings()
    {
        // Override in derived classes to add common mappings
    }

    /// <summary>
    /// Configure entity-specific mappings
    /// </summary>
    protected abstract void ConfigureEntityMappings();

    /// <summary>
    /// Configure DTO-specific mappings
    /// </summary>
    protected abstract void ConfigureDtoMappings();
} 