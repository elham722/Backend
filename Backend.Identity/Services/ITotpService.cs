namespace Backend.Identity.Services
{
    /// <summary>
    /// Interface for TOTP (Time-based One-Time Password) service
    /// </summary>
    public interface ITotpService
    {
        /// <summary>
        /// Generates a new TOTP secret key for a user
        /// </summary>
        string GenerateSecretKey();

        /// <summary>
        /// Generates a QR code URL for authenticator apps
        /// </summary>
        string GenerateQrCodeUrl(string secretKey, string userName, string issuer = "Backend");

        /// <summary>
        /// Validates a TOTP code
        /// </summary>
        bool ValidateCode(string secretKey, string code, int window = 1);

        /// <summary>
        /// Generates the current TOTP code
        /// </summary>
        string GenerateCode(string secretKey);

        /// <summary>
        /// Gets the remaining time for the current code
        /// </summary>
        int GetRemainingSeconds();
    }
} 