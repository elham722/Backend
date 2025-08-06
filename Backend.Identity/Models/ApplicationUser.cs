using System;
using System.Collections.Generic;
using System.Linq;
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

        // Navigation Properties
        public virtual ICollection<UserClaim> UserClaims { get; private set; } = new List<UserClaim>();
        public virtual ICollection<UserLogin> UserLogins { get; private set; } = new List<UserLogin>();
        public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
        public virtual ICollection<UserToken> UserTokens { get; private set; } = new List<UserToken>();

        // Computed Properties
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
        public bool IsAccountLocked => Account.IsLocked();
        public bool IsActive => Account.IsActive && !Account.IsDeleted;
        public bool IsNewUser => DateTime.UtcNow.Subtract(Account.CreatedAt).Days <= 7;

        // Active entities
        public IEnumerable<UserClaim> ActiveClaims => UserClaims.Where(uc => uc.IsActive);
        public IEnumerable<UserLogin> ActiveLogins => UserLogins.Where(ul => ul.IsActive);
        public IEnumerable<UserRole> ActiveRoles => UserRoles.Where(ur => ur.IsActiveAndNotExpired());
        public IEnumerable<UserToken> ValidTokens => UserTokens.Where(ut => ut.IsValid());

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

        // Claims Management
        public UserClaim AddClaim(string claimType, string claimValue, string? createdBy = null)
        {
            var claim = UserClaim.Create(Id, claimType, claimValue, createdBy ?? UserName);
            UserClaims.Add(claim);
            return claim;
        }

        public void RemoveClaim(int claimId, string removedBy)
        {
            var claim = UserClaims.FirstOrDefault(uc => uc.Id == claimId);
            if (claim != null)
            {
                claim.Deactivate(removedBy);
            }
        }

        public void UpdateClaim(int claimId, string newValue, string updatedBy)
        {
            var claim = UserClaims.FirstOrDefault(uc => uc.Id == claimId);
            if (claim != null)
            {
                claim.Update(newValue, updatedBy);
            }
        }

        // Login Management
        public UserLogin AddLogin(string loginProvider, string providerKey, string? deviceInfo = null, string? ipAddress = null, string? userAgent = null)
        {
            var login = UserLogin.Create(Id, loginProvider, providerKey, deviceInfo, ipAddress, userAgent, UserName);
            UserLogins.Add(login);
            return login;
        }

        public void RemoveLogin(string loginProvider, string providerKey, string removedBy)
        {
            var login = UserLogins.FirstOrDefault(ul => ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey);
            if (login != null)
            {
                login.Deactivate(removedBy);
            }
        }

        public void UpdateLoginDeviceInfo(string loginProvider, string providerKey, string? deviceInfo, string? ipAddress, string? userAgent)
        {
            var login = UserLogins.FirstOrDefault(ul => ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey);
            if (login != null)
            {
                login.UpdateDeviceInfo(deviceInfo, ipAddress, userAgent, UserName);
            }
        }

        // Role Management
        public UserRole AddRole(string roleId, string? assignedBy = null, string? assignmentReason = null, DateTime? expiresAt = null)
        {
            var userRole = UserRole.Create(Id, roleId, assignedBy, assignmentReason, expiresAt, UserName);
            UserRoles.Add(userRole);
            return userRole;
        }

        public void RemoveRole(string roleId, string removedBy)
        {
            var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (userRole != null)
            {
                userRole.Deactivate(removedBy);
            }
        }

        public void ExtendRoleExpiration(string roleId, DateTime newExpiresAt)
        {
            var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (userRole != null)
            {
                userRole.ExtendExpiration(newExpiresAt, UserName);
            }
        }

        // Token Management
        public UserToken AddToken(string loginProvider, string name, string value, DateTime? expiresAt = null, string? deviceInfo = null, string? ipAddress = null, string? userAgent = null)
        {
            var token = UserToken.Create(Id, loginProvider, name, value, expiresAt, deviceInfo, ipAddress, userAgent, UserName);
            UserTokens.Add(token);
            return token;
        }

        public void RevokeToken(string loginProvider, string name, string? revocationReason = null)
        {
            var token = UserTokens.FirstOrDefault(ut => ut.LoginProvider == loginProvider && ut.Name == name);
            if (token != null)
            {
                token.Revoke(UserName, revocationReason);
            }
        }

        public void RevokeAllTokens(string? revocationReason = null)
        {
            foreach (var token in ValidTokens)
            {
                token.Revoke(UserName, revocationReason);
            }
        }

        public void RevokeTokensFromDevice(string? deviceInfo, string? ipAddress, string? userAgent, string? revocationReason = null)
        {
            var tokensToRevoke = UserTokens.Where(ut => ut.IsFromSameDevice(deviceInfo, ipAddress, userAgent) && ut.IsValid());
            foreach (var token in tokensToRevoke)
            {
                token.Revoke(UserName, revocationReason);
            }
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

        public bool HasActiveRole(string roleId)
        {
            return ActiveRoles.Any(ur => ur.RoleId == roleId);
        }

        public bool HasClaim(string claimType, string claimValue)
        {
            return ActiveClaims.Any(uc => uc.ClaimType == claimType && uc.ClaimValue == claimValue);
        }

        public bool HasValidToken(string loginProvider, string name)
        {
            return ValidTokens.Any(ut => ut.LoginProvider == loginProvider && ut.Name == name);
        }

        public int GetActiveSessionsCount()
        {
            return ValidTokens.Count();
        }

        public void CleanupExpiredEntities()
        {
            // This method can be called periodically to clean up expired entities
            // In a real implementation, you might want to move expired entities to a separate table
            // or mark them as archived instead of deleting them
        }
    }
}
