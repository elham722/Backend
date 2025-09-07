using System;
using Backend.Identity.ValueObjects;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Interfaces.Identity;
using Backend.Identity.Services;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // Added for List<string>

namespace Backend.Identity.Models
{
    public class ApplicationUser : IdentityUser, IApplicationUser
    {
        // Core Identity Properties
        public AccountInfo Account { get; private set; } = null!;
        public SecurityInfo Security { get; private set; } = null!;
        public AuditInfo Audit { get; private set; } = null!;
        
        // Explicit interface implementations for proper return types
        IAccountInfo IApplicationUser.Account => Account;
        ISecurityInfo IApplicationUser.Security => Security;
        IAuditInfo IApplicationUser.Audit => Audit;

        // Integration with Domain (using Guid instead of string)
        public Guid? CustomerId { get; private set; }
        
        // MFA Properties
        public string? TotpSecretKey { get; private set; }
        public bool TotpEnabled { get; private set; } = false;
        public bool SmsEnabled { get; private set; } = false;
        
        // Social Login Properties
        public string? GoogleId { get; private set; }
        public string? MicrosoftId { get; private set; }

        // Navigation property for roles (not mapped to database, used for DTO mapping)
        public List<string> Roles { get; set; } = new();

        // Computed Properties
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
        public bool IsAccountLocked => Account.IsLocked();
        public bool IsActive => Account.IsActive && !Account.IsDeleted;
        public bool IsNewUser => DateTime.UtcNow.Subtract(Account.CreatedAt).Days <= 7;
        
        // Additional Properties for IApplicationUser interface
        public bool IsDeleted => Account.IsDeleted;
        public DateTime? LastLoginAt => Account.LastLoginAt;
        public DateTime? LastPasswordChangeAt => Account.LastPasswordChangeAt;
        public int LoginAttempts => Account.LoginAttempts;
        public bool RequiresPasswordChange => Account.LastPasswordChangeAt == null || 
            DateTime.UtcNow.Subtract(Account.LastPasswordChangeAt.Value).Days > 90;

        private ApplicationUser() 
        { 
            // Initialize value objects for EF Core
            Account = AccountInfo.Create();
            Security = SecurityInfo.Create();
            Audit = AuditInfo.Create();
        } // For EF Core

        public static ApplicationUser Create(string email, string userName, Guid? customerId = null, string? createdBy = null, IDateTimeService? dateTimeService = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Username cannot be null or empty", nameof(userName));

            // Use provided dateTimeService or fallback to DateTime.UtcNow for backward compatibility
            var dtService = dateTimeService ?? new DefaultDateTimeService();

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
                Account = AccountInfo.Create(dtService),
                Security = SecurityInfo.Create(),
                Audit = AuditInfo.Create(createdBy)
            };
        }

        // Internal methods for services to update user state
        internal void UpdateAccount(AccountInfo account)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
        }

        internal void UpdateSecurity(SecurityInfo security)
        {
            Security = security ?? throw new ArgumentNullException(nameof(security));
        }

        internal void UpdateAudit(AuditInfo audit)
        {
            Audit = audit ?? throw new ArgumentNullException(nameof(audit));
        }

        internal void SetCustomerId(Guid? customerId)
        {
            CustomerId = customerId;
        }

        internal void SetTotpSecretKey(string? secretKey)
        {
            TotpSecretKey = secretKey;
        }

        internal void SetTotpEnabled(bool enabled)
        {
            TotpEnabled = enabled;
        }

        internal void SetSmsEnabled(bool enabled)
        {
            SmsEnabled = enabled;
        }

        internal void SetGoogleId(string? googleId)
        {
            GoogleId = googleId;
        }

        internal void SetMicrosoftId(string? microsoftId)
        {
            MicrosoftId = microsoftId;
        }

        // Business Logic Methods
        public bool CanLogin()
        {
            return IsActive && !IsLocked && !IsAccountLocked;
        }


        // Note: For claims, roles, logins, and tokens management, use UserManager and RoleManager methods
        // These operations should be handled at the service layer, not in the entity
    }
}
