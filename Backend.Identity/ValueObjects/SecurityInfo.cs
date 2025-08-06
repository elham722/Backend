using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Identity.ValueObjects
{
    public class SecurityInfo
    {
        public bool TwoFactorEnabled { get; private set; }
        public string? TwoFactorSecret { get; private set; }
        public DateTime? TwoFactorEnabledAt { get; private set; }

        private SecurityInfo() { } // For EF Core

        public static SecurityInfo Create()
        {
            return new SecurityInfo
            {
                TwoFactorEnabled = false,
                TwoFactorSecret = null,
                TwoFactorEnabledAt = null
            };
        }

        public SecurityInfo EnableTwoFactor(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Two-factor secret cannot be null or empty", nameof(secret));

            return new SecurityInfo
            {
                TwoFactorEnabled = true,
                TwoFactorSecret = secret,
                TwoFactorEnabledAt = DateTime.UtcNow
            };
        }

        public SecurityInfo DisableTwoFactor()
        {
            return new SecurityInfo
            {
                TwoFactorEnabled = false,
                TwoFactorSecret = null,
                TwoFactorEnabledAt = null
            };
        }

        public SecurityInfo UpdateTwoFactorSecret(string newSecret)
        {
            if (string.IsNullOrWhiteSpace(newSecret))
                throw new ArgumentException("Two-factor secret cannot be null or empty", nameof(newSecret));

            if (!TwoFactorEnabled)
                throw new InvalidOperationException("Two-factor authentication is not enabled");

            return new SecurityInfo
            {
                TwoFactorEnabled = this.TwoFactorEnabled,
                TwoFactorSecret = newSecret,
                TwoFactorEnabledAt = this.TwoFactorEnabledAt
            };
        }

        public bool ValidateTwoFactorSecret(string secret)
        {
            return TwoFactorEnabled && TwoFactorSecret == secret;
        }
    }
}
