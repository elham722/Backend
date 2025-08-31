using Backend.Application.Common.Commands;
using Backend.Application.Features.UserManagement.DTOs;
using Backend.Domain.Enums;

namespace Backend.Application.Features.UserManagement.Commands.MFA;

/// <summary>
/// Command to verify MFA code for a user
/// </summary>
public class VerifyMfaCommand : ICommand<MfaVerificationResultDto>
{
    public string UserId { get; set; } = string.Empty;
    public MfaType Type { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public bool RememberDevice { get; set; }
} 