using AutoMapper;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Identity.Models;

namespace Backend.Identity.Mappers;

/// <summary>
/// AutoMapper profile for user-related mappings
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // ApplicationUser to UserDto
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName ?? string.Empty))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => src.TwoFactorEnabled))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.Account.IsDeleted))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => src.Account.LastLoginAt))
            .ForMember(dest => dest.LastPasswordChangeAt, opt => opt.MapFrom(src => src.Account.LastPasswordChangeAt))
            .ForMember(dest => dest.LoginAttempts, opt => opt.MapFrom(src => src.Account.LoginAttempts))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.IsNewUser, opt => opt.MapFrom(src => src.IsNewUser))
            .ForMember(dest => dest.RequiresPasswordChange, opt => opt.MapFrom(src => src.RequiresPasswordChange(90)))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Account.CreatedAt))
            .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(src => src.Audit.UpdatedAt))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => GetUserStatus(src)))
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles will be populated separately

        // CreateUserDto to ApplicationUser (for creation)
        CreateMap<CreateUserDto, ApplicationUser>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            // Password handling: Password should be handled separately using UserManager.CreateAsync(user, password)
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Never map password directly
            
            .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Roles handled separately
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsNewUser, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore())
            .ForMember(dest => dest.Security, opt => opt.Ignore())
            .ForMember(dest => dest.Audit, opt => opt.Ignore())
            .ForMember(dest => dest.TotpSecretKey, opt => opt.Ignore())
            .ForMember(dest => dest.TotpEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.SmsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.GoogleId, opt => opt.Ignore())
            .ForMember(dest => dest.MicrosoftId, opt => opt.Ignore());

        // UpdateUserDto to ApplicationUser (for updates)
        CreateMap<UpdateUserDto, ApplicationUser>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.PhoneNumberConfirmed))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
            // Password handling: Password updates should be handled separately using UserManager.ChangePasswordAsync
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Never map password directly
          
            .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Roles handled separately
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsNewUser, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore())
            .ForMember(dest => dest.Security, opt => opt.Ignore())
            .ForMember(dest => dest.Audit, opt => opt.Ignore())
            .ForMember(dest => dest.TotpSecretKey, opt => opt.Ignore())
            .ForMember(dest => dest.TotpEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.SmsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.GoogleId, opt => opt.Ignore())
            .ForMember(dest => dest.MicrosoftId, opt => opt.Ignore());

        // RegisterDto to ApplicationUser (for registration)
        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            // Password handling: Password should be handled separately using UserManager.CreateAsync(user, password)
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Never map password directly
           
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsNewUser, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore())
            .ForMember(dest => dest.Security, opt => opt.Ignore())
            .ForMember(dest => dest.Audit, opt => opt.Ignore())
            .ForMember(dest => dest.TotpSecretKey, opt => opt.Ignore())
            .ForMember(dest => dest.TotpEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.SmsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.GoogleId, opt => opt.Ignore())
            .ForMember(dest => dest.MicrosoftId, opt => opt.Ignore());

        // UserProfileDto to ApplicationUser (for profile updates)
        CreateMap<UserProfileDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            // Password handling: Password changes should be handled separately using UserManager.ChangePasswordAsync
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Never map password directly
           
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsNewUser, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore())
            .ForMember(dest => dest.Security, opt => opt.Ignore())
            .ForMember(dest => dest.Audit, opt => opt.Ignore())
            .ForMember(dest => dest.TotpSecretKey, opt => opt.Ignore())
            .ForMember(dest => dest.TotpEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.SmsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.GoogleId, opt => opt.Ignore())
            .ForMember(dest => dest.MicrosoftId, opt => opt.Ignore());
    }

    private static string GetUserStatus(ApplicationUser user)
    {
        if (!user.IsActive)
            return "Inactive";
        if (user.IsLocked)
            return "Locked";
        if (user.IsAccountLocked)
            return "AccountLocked";
        return "Active";
    }
}
