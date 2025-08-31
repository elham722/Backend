using Backend.Application.Common.Commands;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Domain.Enums;

namespace Backend.Application.Features.UserManagement.Commands.MFA;

/// <summary>
/// Command to setup MFA for a user
/// </summary>
public class SetupMfaCommand : ICommand<MfaSetupDto>
{
    public string UserId { get; set; } = string.Empty;
    public MfaType Type { get; set; }
    public string? PhoneNumber { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
} 