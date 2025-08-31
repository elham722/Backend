using Backend.Application.Common.Commands;
using Backend.Domain.Enums;

namespace Backend.Application.Features.UserManagement.Commands.MFA;

/// <summary>
/// Command to disable MFA for a user
/// </summary>
public class DisableMfaCommand : ICommand<bool>
{
    public string UserId { get; set; } = string.Empty;
    public MfaType Type { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
} 