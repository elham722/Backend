using AutoMapper;
using Backend.Identity.Models;
using Backend.Application.Features.UserManagement.DTOs;

namespace Backend.Infrastructure.Mappers;

/// <summary>
/// AutoMapper profile for Identity concrete models to DTOs
/// This profile is in Infrastructure layer to avoid Application layer dependency on Identity models
/// </summary>
public class IdentityConcreteMappingProfile : Profile
{
    public IdentityConcreteMappingProfile()
    {
        // ApplicationUser (concrete class) to UserDto
        CreateMap<ApplicationUser, UserDto>()
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
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.Account.IsDeleted))
            .ForMember(dest => dest.IsNewUser, opt => opt.MapFrom(src => src.IsNewUser))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.Account.LastLoginAt))
            .ForMember(dest => dest.LastPasswordChangeAt, opt => opt.MapFrom(src => src.Account.LastPasswordChangeAt))
            .ForMember(dest => dest.LoginAttempts, opt => opt.MapFrom(src => src.Account.LoginAttempts))
            .ForMember(dest => dest.RequiresPasswordChange, opt => opt.MapFrom(src => false)) // Default to false
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.Audit.CreatedBy))
            .ForMember(dest => dest.LastModifiedAt, opt => opt.MapFrom(src => src.Audit.UpdatedAt))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.Audit.UpdatedBy));

        // ApplicationUser to UserSummaryDto
        CreateMap<ApplicationUser, UserSummaryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt));
    }
}