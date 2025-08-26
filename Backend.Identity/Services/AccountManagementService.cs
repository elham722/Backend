using System;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Identity.Services
{
    public interface IAccountManagementService
    {
        Task<bool> UpdateLastLoginAsync(ApplicationUser user);
        Task<bool> UpdatePasswordChangeAsync(ApplicationUser user);
        Task<bool> IncrementLoginAttemptsAsync(ApplicationUser user);
        Task<bool> DeactivateAccountAsync(ApplicationUser user);
        Task<bool> ActivateAccountAsync(ApplicationUser user);
        Task<bool> MarkAsDeletedAsync(ApplicationUser user);
        Task<bool> LinkToCustomerAsync(ApplicationUser user, Guid customerId);
        Task<bool> UnlinkFromCustomerAsync(ApplicationUser user);
    }

    public class AccountManagementService : IAccountManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDateTimeService _dateTimeService;

        public AccountManagementService(
            UserManager<ApplicationUser> userManager, 
            IDateTimeService dateTimeService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        public async Task<bool> UpdateLastLoginAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.UpdateLastLogin(_dateTimeService);
            user.UpdateAccount(updatedAccount);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UpdatePasswordChangeAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.UpdatePasswordChange(_dateTimeService);
            user.UpdateAccount(updatedAccount);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> IncrementLoginAttemptsAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.IncrementLoginAttempts();
            user.UpdateAccount(updatedAccount);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeactivateAccountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.Deactivate();
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateAccount(updatedAccount);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ActivateAccountAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.Activate();
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateAccount(updatedAccount);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> MarkAsDeletedAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var updatedAccount = user.Account.MarkAsDeleted(_dateTimeService);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            
            user.UpdateAccount(updatedAccount);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> LinkToCustomerAsync(ApplicationUser user, Guid customerId)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetCustomerId(customerId);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> UnlinkFromCustomerAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.SetCustomerId(null);
            var updatedAudit = user.Audit.Update(user.UserName, _dateTimeService);
            user.UpdateAudit(updatedAudit);
            
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
} 