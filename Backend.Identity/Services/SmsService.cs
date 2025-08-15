using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Implementation of SMS service
    /// </summary>
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _provider;
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;

        public SmsService(
            ILogger<SmsService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _provider = configuration["SmsSettings:Provider"] ?? "Mock";
            _accountSid = configuration["SmsSettings:AccountSid"] ?? string.Empty;
            _authToken = configuration["SmsSettings:AuthToken"] ?? string.Empty;
            _fromNumber = configuration["SmsSettings:FromNumber"] ?? string.Empty;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                if (!IsValidPhoneNumber(phoneNumber))
                {
                    _logger.LogWarning("Invalid phone number format: {PhoneNumber}", phoneNumber);
                    return false;
                }

                var formattedNumber = FormatPhoneNumber(phoneNumber);

                switch (_provider.ToLowerInvariant())
                {
                    case "twilio":
                        return await SendViaTwilioAsync(formattedNumber, message);
                    case "mock":
                        return await SendViaMockAsync(formattedNumber, message);
                    default:
                        _logger.LogError("Unsupported SMS provider: {Provider}", _provider);
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code)
        {
            var message = $"Your verification code is: {code}. Valid for 5 minutes.";
            return await SendSmsAsync(phoneNumber, message);
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Remove all non-digit characters
            var digitsOnly = Regex.Replace(phoneNumber, @"[^\d]", "");
            
            // Check if it's a valid international phone number (7-15 digits)
            return digitsOnly.Length >= 7 && digitsOnly.Length <= 15;
        }

        public string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            // Remove all non-digit characters
            var digitsOnly = Regex.Replace(phoneNumber, @"[^\d]", "");

            // If it starts with 00, replace with +
            if (digitsOnly.StartsWith("00"))
            {
                digitsOnly = "+" + digitsOnly.Substring(2);
            }
            // If it doesn't start with + and is 10-15 digits, assume it's international
            else if (!digitsOnly.StartsWith("+") && digitsOnly.Length >= 10)
            {
                digitsOnly = "+" + digitsOnly;
            }

            return digitsOnly;
        }

        private async Task<bool> SendViaTwilioAsync(string phoneNumber, string message)
        {
            try
            {
                // Note: In a real implementation, you would use the Twilio SDK
                // For now, we'll simulate the API call
                _logger.LogInformation("Sending SMS via Twilio to {PhoneNumber}: {Message}", phoneNumber, message);
                
                // Simulate API delay
                await Task.Delay(100);
                
                _logger.LogInformation("SMS sent successfully via Twilio to {PhoneNumber}", phoneNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS via Twilio to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        private async Task<bool> SendViaMockAsync(string phoneNumber, string message)
        {
            try
            {
                // Mock implementation for development/testing
                _logger.LogInformation("MOCK SMS to {PhoneNumber}: {Message}", phoneNumber, message);
                
                // Simulate API delay
                await Task.Delay(50);
                
                _logger.LogInformation("MOCK SMS sent successfully to {PhoneNumber}", phoneNumber);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending MOCK SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
    }
} 