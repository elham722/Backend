using Backend.Domain.Enums;

namespace Backend.Application.Features.UserManagement.DTOs;

/// <summary>
/// DTO for MFA setup information
/// </summary>
public class MfaSetupDto
{
    public string UserId { get; set; } = string.Empty;
    public MfaType Type { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? LastUsed { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }

    // TOTP specific properties
    public string? TotpSecretKey { get; set; }
    public string? TotpQrCodeUrl { get; set; }
    public int TotpDigits { get; set; }
    public int TotpPeriod { get; set; }

    // SMS specific properties
    public string? PhoneNumber { get; set; }
    public DateTime? SmsCodeExpiry { get; set; }

    // Backup codes
    public List<string> BackupCodes { get; set; } = new();
    public int RemainingBackupCodes { get; set; }

    // Setup status
    public bool IsSetupComplete { get; set; }
    public string? SetupMessage { get; set; }
    public List<string> SetupSteps { get; set; } = new();
} 