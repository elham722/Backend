using Backend.Domain.Enums;

namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for MFA verification
/// </summary>
public class MfaVerificationDto
{
    public string UserId { get; set; } = string.Empty;
    public MfaType Type { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public bool RememberDevice { get; set; }
}

/// <summary>
/// DTO for MFA verification result
/// </summary>
public class MfaVerificationResultDto
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsValid { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresAdditionalVerification { get; set; }
    public List<MfaType> AvailableMethods { get; set; } = new();
} 