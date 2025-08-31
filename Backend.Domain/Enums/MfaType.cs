namespace Backend.Domain.Enums;

/// <summary>
/// Types of Multi-Factor Authentication methods
/// </summary>
public enum MfaType
{
    /// <summary>
    /// Time-based One-Time Password (Google Authenticator, Microsoft Authenticator)
    /// </summary>
    TOTP = 1,

    /// <summary>
    /// SMS verification codes
    /// </summary>
    SMS = 2,

    /// <summary>
    /// Backup codes for account recovery
    /// </summary>
    BackupCodes = 3,

    /// <summary>
    /// Email verification codes
    /// </summary>
    Email = 4,

    /// <summary>
    /// Hardware security keys (FIDO2/U2F)
    /// </summary>
    HardwareKey = 5
} 