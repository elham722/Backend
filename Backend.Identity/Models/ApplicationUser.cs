using System;
using Backend.Identity.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        public AccountInfo Account { get; private set; } = null!;
        public SecurityInfo Security { get; private set; } = null!;
        public AuditInfo Audit { get; private set; } = null!;

        // Integration with Domain
        public string? CustomerId { get; private set; }
        
        // MFA Properties
        public string? TotpSecretKey { get; private set; }
        public bool TotpEnabled { get; private set; } = false;
        public bool SmsEnabled { get; private set; } = false;
        
        // Social Login Properties
        public string? GoogleId { get; private set; }
        public string? MicrosoftId { get; private set; }

        // Computed Properties
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
        public bool IsAccountLocked => Account.IsLocked();
        public bool IsActive => Account.IsActive && !Account.IsDeleted;
        public bool IsNewUser => DateTime.UtcNow.Subtract(Account.CreatedAt).Days <= 7;

        private ApplicationUser() { } // For EF Core

        public static ApplicationUser Create(string email, string userName, string? customerId = null, string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Username cannot be null or empty", nameof(userName));

            return new ApplicationUser
            {
                Email = email,
                UserName = userName,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                CustomerId = customerId,
                Account = AccountInfo.Create(),
                Security = SecurityInfo.Create(),
                Audit = AuditInfo.Create(createdBy)
            };
        }

        // Account Management Methods
        public void UpdateLastLogin()
        {
            Account = Account.UpdateLastLogin();
        }

        public void UpdatePasswordChange()
        {
            Account = Account.UpdatePasswordChange();
        }

        public void IncrementLoginAttempts()
        {
            Account = Account.IncrementLoginAttempts();
        }

        public void DeactivateAccount()
        {
            Account = Account.Deactivate();
            Audit = Audit.Update(UserName);
        }

        public void ActivateAccount()
        {
            Account = Account.Activate();
            Audit = Audit.Update(UserName);
        }

        public void MarkAsDeleted()
        {
            Account = Account.MarkAsDeleted();
            Audit = Audit.Update(UserName);
        }

        // Security Management Methods
        public void EnableTwoFactor(string secret)
        {
            Security = Security.EnableTwoFactor(secret);
            Audit = Audit.Update(UserName);
        }

        public void DisableTwoFactor()
        {
            Security = Security.DisableTwoFactor();
            Audit = Audit.Update(UserName);
        }

        public void UpdateTwoFactorSecret(string newSecret)
        {
            Security = Security.UpdateTwoFactorSecret(newSecret);
            Audit = Audit.Update(UserName);
        }

        public bool ValidateTwoFactorSecret(string secret)
        {
            return Security.ValidateTwoFactorSecret(secret);
        }

        // Customer Integration
        public void LinkToCustomer(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

            CustomerId = customerId;
            Audit = Audit.Update(UserName);
        }

        public void UnlinkFromCustomer()
        {
            CustomerId = null;
            Audit = Audit.Update(UserName);
        }

        // MFA Management Methods
        public void EnableTotp(string secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("TOTP secret key cannot be null or empty", nameof(secretKey));

            TotpSecretKey = secretKey;
            TotpEnabled = true;
            Audit = Audit.Update(UserName);
        }

        public void DisableTotp()
        {
            TotpSecretKey = null;
            TotpEnabled = false;
            Audit = Audit.Update(UserName);
        }

        public void EnableSms()
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber))
                throw new InvalidOperationException("Phone number must be set before enabling SMS");

            SmsEnabled = true;
            Audit = Audit.Update(UserName);
        }

        public void DisableSms()
        {
            SmsEnabled = false;
            Audit = Audit.Update(UserName);
        }

        // Social Login Management Methods
        public void LinkGoogleAccount(string googleId)
        {
            if (string.IsNullOrWhiteSpace(googleId))
                throw new ArgumentException("Google ID cannot be null or empty", nameof(googleId));

            GoogleId = googleId;
            Audit = Audit.Update(UserName);
        }

        public void UnlinkGoogleAccount()
        {
            GoogleId = null;
            Audit = Audit.Update(UserName);
        }

        public void LinkMicrosoftAccount(string microsoftId)
        {
            if (string.IsNullOrWhiteSpace(microsoftId))
                throw new ArgumentException("Microsoft ID cannot be null or empty", nameof(microsoftId));

            MicrosoftId = microsoftId;
            Audit = Audit.Update(UserName);
        }

        public void UnlinkMicrosoftAccount()
        {
            MicrosoftId = null;
            Audit = Audit.Update(UserName);
        }

        // Business Logic Methods
        public bool CanLogin()
        {
            return IsActive && !IsLocked && !IsAccountLocked;
        }

        public bool RequiresPasswordChange(int maxPasswordAgeDays = 90)
        {
            if (!Account.LastPasswordChangeAt.HasValue)
                return true;

            return DateTime.UtcNow.Subtract(Account.LastPasswordChangeAt.Value).Days > maxPasswordAgeDays;
        }

        // Note: For claims, roles, logins, and tokens management, use UserManager and RoleManager methods
        // These operations should be handled at the service layer, not in the entity
    }
}
