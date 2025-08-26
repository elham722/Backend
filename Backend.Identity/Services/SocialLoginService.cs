using System;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Services
{
    public interface ISocialLoginService
    {
        Task<bool> LinkGoogleAccountAsync(ApplicationUser user, string googleId);
        Task<bool> UnlinkGoogleAccountAsync(ApplicationUser user);
        Task<bool> LinkMicrosoftAccountAsync(ApplicationUser user, string microsoftId);
        Task<bool> UnlinkMicrosoftAccountAsync(ApplicationUser user);
        Task<bool> IsGoogleLinkedAsync(ApplicationUser user);
        Task<bool> IsMicrosoftLinkedAsync(ApplicationUser user);
    }

    public class SocialLoginService : ISocialLoginService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDateTimeService _dateTimeService;

        public SocialLoginService(UserManager<ApplicationUser> userManager, IDateTimeService dateTimeService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        public async Task<bool> LinkGoogleAccountAsync(ApplicationUser user, string googleId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(googleId))
                throw new ArgumentException("Google ID cannot be null or empty", nameof(googleId));

            user.SetGoogleId(googleId);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UnlinkGoogleAccountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetGoogleId(null);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> LinkMicrosoftAccountAsync(ApplicationUser user, string microsoftId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(microsoftId))
                throw new ArgumentException("Microsoft ID cannot be null or empty", nameof(microsoftId));

            user.SetMicrosoftId(microsoftId);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UnlinkMicrosoftAccountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetMicrosoftId(null);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> IsGoogleLinkedAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return !string.IsNullOrWhiteSpace(user.GoogleId);
        }

        public async Task<bool> IsMicrosoftLinkedAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return !string.IsNullOrWhiteSpace(user.MicrosoftId);
        }
    }
} 