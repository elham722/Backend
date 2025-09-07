using AutoMapper;
using Backend.Application.Common.Interfaces.Identity;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Application.Features.UserManagement.DTOs.Identity;

namespace Backend.Application.Mappers.Profiles;

/// <summary>
/// AutoMapper profile for Identity-related mappings
/// This profile maps from Identity interfaces to DTOs, avoiding direct dependency on Identity layer
/// </summary>
public class IdentityMappingProfile : BaseMappingProfile
{
    protected override void ConfigureEntityMappings()
    {
        // Entity to Entity mappings (if any)
    }

    protected override void ConfigureDtoMappings()
    {
        // ApplicationUser to UserDto
        CreateMap<IApplicationUser, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled))
            .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => src.LockoutEnabled))
            .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => src.AccessFailedCount))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.TotpSecretKey, opt => opt.MapFrom(src => src.TotpSecretKey))
            .ForMember(dest => dest.TotpEnabled, opt => opt.MapFrom(src => src.TotpEnabled))
            .ForMember(dest => dest.SmsEnabled, opt => opt.MapFrom(src => src.SmsEnabled))
            .ForMember(dest => dest.GoogleId, opt => opt.MapFrom(src => src.GoogleId))
            .ForMember(dest => dest.MicrosoftId, opt => opt.MapFrom(src => src.MicrosoftId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.IsNewUser, opt => opt.MapFrom(src => src.IsNewUser))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.LastLoginAt))
            .ForMember(dest => dest.LastPasswordChangeAt, opt => opt.MapFrom(src => src.LastPasswordChangeAt))
            .ForMember(dest => dest.LoginAttempts, opt => opt.MapFrom(src => src.LoginAttempts))
            .ForMember(dest => dest.RequiresPasswordChange, opt => opt.MapFrom(src => src.RequiresPasswordChange))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Audit.CreatedAt))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.Audit.CreatedBy))
            .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.Audit.LastModifiedAt))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.Audit.LastModifiedBy));

        // UserClaim to UserClaimDto
        CreateMap<IUserClaim, UserClaimDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.ClaimType, opt => opt.MapFrom(src => src.ClaimType))
            .ForMember(dest => dest.ClaimValue, opt => opt.MapFrom(src => src.ClaimValue));

        // UserToken to UserTokenDto
        CreateMap<IUserToken, UserTokenDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.LoginProvider, opt => opt.MapFrom(src => src.LoginProvider))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => src.ExpiresAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        // UserRole to UserRoleDto
        CreateMap<IUserRole, UserRoleDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleName));

        // UserLogin to UserLoginDto
        CreateMap<IUserLogin, UserLoginDto>()
            .ForMember(dest => dest.LoginProvider, opt => opt.MapFrom(src => src.LoginProvider))
            .ForMember(dest => dest.ProviderKey, opt => opt.MapFrom(src => src.ProviderKey))
            .ForMember(dest => dest.ProviderDisplayName, opt => opt.MapFrom(src => src.ProviderDisplayName))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
    }
}