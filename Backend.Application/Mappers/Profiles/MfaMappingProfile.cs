using AutoMapper;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Domain.Entities.MFA;

namespace Backend.Application.Mappers.Profiles;

/// <summary>
/// AutoMapper profile for MFA-related mappings
/// </summary>
public class MfaMappingProfile : BaseMappingProfile
{
    protected override void ConfigureEntityMappings()
    {
        // Entity to Entity mappings (if any)
    }

    protected override void ConfigureDtoMappings()
    {
        CreateMap<MfaMethod, MfaSetupDto>()
            .ForMember(dest => dest.IsSetupComplete, opt => opt.MapFrom(src => src.IsEnabled))
            .ForMember(dest => dest.SetupMessage, opt => opt.MapFrom(src => GetSetupMessage(src)))
            .ForMember(dest => dest.SetupSteps, opt => opt.MapFrom(src => GetSetupSteps(src)));
    }

    private static string GetSetupMessage(MfaMethod mfa)
    {
        if (mfa.IsEnabled)
            return $"{mfa.Type} MFA is already enabled";

        return mfa.Type switch
        {
            Domain.Enums.MfaType.TOTP => "Scan the QR code with your authenticator app",
            Domain.Enums.MfaType.SMS => "Enter your phone number to receive SMS codes",
            Domain.Enums.MfaType.BackupCodes => "Generate backup codes for account recovery",
            _ => "Complete the setup process"
        };
    }

    private static List<string> GetSetupSteps(MfaMethod mfa)
    {
        if (mfa.IsEnabled)
            return new List<string> { "MFA is already configured" };

        return mfa.Type switch
        {
            Domain.Enums.MfaType.TOTP => new List<string>
            {
                "Scan the QR code with Google Authenticator or Microsoft Authenticator",
                "Enter the 6-digit code from your app",
                "Click Enable to activate TOTP MFA"
            },
            Domain.Enums.MfaType.SMS => new List<string>
            {
                "Enter your phone number",
                "Click Send Code to receive SMS",
                "Enter the 6-digit code from SMS",
                "Click Enable to activate SMS MFA"
            },
            Domain.Enums.MfaType.BackupCodes => new List<string>
            {
                "Generate 10 backup codes",
                "Save codes in a secure location",
                "Click Enable to activate backup codes"
            },
            _ => new List<string> { "Follow the setup instructions" }
        };
    }
} 