using System;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Services
{
    public interface ITwoFactorService
    {
        Task<bool> EnableTotpAsync(ApplicationUser user, string secretKey);
        Task<bool> DisableTotpAsync(ApplicationUser user);
        Task<bool> EnableSmsAsync(ApplicationUser user);
        Task<bool> DisableSmsAsync(ApplicationUser user);
        Task<bool> ValidateTotpAsync(ApplicationUser user, string token);
        Task<bool> ValidateSmsAsync(ApplicationUser user, string token);
        Task<string> GenerateSmsTokenAsync(ApplicationUser user);
        Task<string> GenerateTotpTokenAsync(ApplicationUser user);
    }

    public class TwoFactorService : ITwoFactorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDateTimeService _dateTimeService;

        public TwoFactorService(UserManager<ApplicationUser> userManager, IDateTimeService dateTimeService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        public async Task<bool> EnableTotpAsync(ApplicationUser user, string secretKey)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("TOTP secret key cannot be null or empty", nameof(secretKey));

            user.SetTotpSecretKey(secretKey);
            user.SetTotpEnabled(true);
            
            var updatedSecurity = user.Security.EnableTwoFactor(secretKey, _dateTimeService);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateSecurity(updatedSecurity);
            user.UpdateAudit(updatedAudit);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DisableTotpAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetTotpSecretKey(null);
            user.SetTotpEnabled(false);
            
            var updatedSecurity = user.Security.DisableTwoFactor();
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateSecurity(updatedSecurity);
            user.UpdateAudit(updatedAudit);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> EnableSmsAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                throw new InvalidOperationException("Phone number must be set before enabling SMS");

            user.SetSmsEnabled(true);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DisableSmsAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetSmsEnabled(false);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ValidateTotpAsync(ApplicationUser user, string token)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(token))
                return false;

            if (!user.TotpEnabled || string.IsNullOrWhiteSpace(user.TotpSecretKey))
                return false;

            // Use UserManager's built-in TOTP validation
            return await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, token);
        }

        public async Task<bool> ValidateSmsAsync(ApplicationUser user, string token)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(token))
                return false;

            if (!user.SmsEnabled)
                return false;

            // Use UserManager's built-in SMS validation with the correct provider name
            // The default SMS token provider is "Phone"
            return await _userManager.VerifyTwoFactorTokenAsync(user, "Phone", token);
        }

        public async Task<string> GenerateSmsTokenAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!user.SmsEnabled)
                throw new InvalidOperationException("SMS two-factor authentication is not enabled for this user");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                throw new InvalidOperationException("Phone number is required for SMS token generation");

            // Generate SMS token using UserManager
            return await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
        }

        public async Task<string> GenerateTotpTokenAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!user.TotpEnabled)
                throw new InvalidOperationException("TOTP two-factor authentication is not enabled for this user");

            // Generate TOTP token using UserManager
            return await _userManager.GenerateTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider);
        }
    }
} 