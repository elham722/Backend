using OtpNet;
using System.Security.Cryptography;

namespace Backend.Identity.Services
{
    /// <summary>
    /// Implementation of TOTP service
    /// </summary>
    public class TotpService : ITotpService
    {
        private const int Digits = 6;
        private const int Period = 30; // 30 seconds

        public string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20); // 160 bits
            return Base32Encoding.ToString(key);
        }

        public string GenerateQrCodeUrl(string secretKey, string userName, string issuer = "Backend")
        {
            var encodedIssuer = Uri.EscapeDataString(issuer);
            var encodedUserName = Uri.EscapeDataString(userName);
            var encodedSecret = Uri.EscapeDataString(secretKey);

            return $"otpauth://totp/{encodedIssuer}:{encodedUserName}?secret={encodedSecret}&issuer={encodedIssuer}&algorithm=SHA1&digits={Digits}&period={Period}";
        }

        public bool ValidateCode(string secretKey, string code, int window = 1)
        {
            try
            {
                var key = Base32Encoding.ToBytes(secretKey);
                var totp = new Totp(key, step: Period, totpSize: Digits);

                var verificationWindow = new VerificationWindow(previous: window, future: window);
                return totp.VerifyTotp(code, out _, verificationWindow);
            }
            catch
            {
                return false;
            }
        }

        public string GenerateCode(string secretKey)
        {
            try
            {
                var key = Base32Encoding.ToBytes(secretKey);
                var totp = new Totp(key, step: Period, totpSize: Digits);

                return totp.ComputeTotp();
            }
            catch
            {
                return string.Empty;
            }
        }

        public int GetRemainingSeconds()
        {
            var epochTicks = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = (long)(epochTicks / Period);
            var nextStep = (timeStep + 1) * Period;
            var remaining = nextStep - epochTicks;

            return (int)remaining;
        }
    }
} 