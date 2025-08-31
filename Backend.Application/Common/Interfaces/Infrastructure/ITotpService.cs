namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Service for TOTP (Time-based One-Time Password) operations
/// </summary>
public interface ITotpService
{
    /// <summary>
    /// Generate a TOTP secret key
    /// </summary>
    string GenerateSecretKey();

    /// <summary>
    /// Generate a TOTP code for the current time
    /// </summary>
    string GenerateCode(string secretKey);

    /// <summary>
    /// Generate a TOTP code for a specific time
    /// </summary>
    string GenerateCode(string secretKey, DateTime time);

    /// <summary>
    /// Validate a TOTP code
    /// </summary>
    bool ValidateCode(string secretKey, string code, int window = 1);

    /// <summary>
    /// Generate QR code URL for authenticator apps
    /// </summary>
    string GenerateQrCodeUrl(string secretKey, string accountName, string issuer);

    /// <summary>
    /// Get remaining time for current TOTP period
    /// </summary>
    int GetRemainingSeconds();
} 