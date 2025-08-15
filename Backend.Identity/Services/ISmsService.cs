namespace Backend.Identity.Services
{
    /// <summary>
    /// Interface for SMS service
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Sends an SMS message
        /// </summary>
        Task<bool> SendSmsAsync(string phoneNumber, string message);

        /// <summary>
        /// Sends a verification code via SMS
        /// </summary>
        Task<bool> SendVerificationCodeAsync(string phoneNumber, string code);

        /// <summary>
        /// Validates a phone number format
        /// </summary>
        bool IsValidPhoneNumber(string phoneNumber);

        /// <summary>
        /// Formats a phone number to international format
        /// </summary>
        string FormatPhoneNumber(string phoneNumber);
    }
} 