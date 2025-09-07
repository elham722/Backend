using System;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Interfaces.Identity;

namespace Backend.Identity.ValueObjects
{
    public class AccountInfo : IAccountInfo
    {
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public DateTime? LastPasswordChangeAt { get; private set; }
        public int LoginAttempts { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? BranchId { get; private set; }

        private AccountInfo() { } // For EF Core

        public static AccountInfo Create(IDateTimeService dateTimeService, string? branchId = null)
        {
            if (dateTimeService == null)
                throw new ArgumentNullException(nameof(dateTimeService));

            return new AccountInfo
            {
                CreatedAt = dateTimeService.UtcNow,
                LoginAttempts = 0,
                IsActive = true,
                IsDeleted = false,
                BranchId = branchId
            };
        }

        public static AccountInfo Create(string? branchId = null)
        {
            return new AccountInfo
            {
                CreatedAt = DateTime.UtcNow,
                LoginAttempts = 0,
                IsActive = true,
                IsDeleted = false,
                BranchId = branchId
            };
        }

        public AccountInfo UpdateLastLogin(IDateTimeService dateTimeService)
        {
            if (dateTimeService == null)
                throw new ArgumentNullException(nameof(dateTimeService));

            return new AccountInfo
            {
                CreatedAt = this.CreatedAt,
                LastLoginAt = dateTimeService.UtcNow,
                LastPasswordChangeAt = this.LastPasswordChangeAt,
                LoginAttempts = 0, // Reset on successful login
                IsActive = this.IsActive,
                IsDeleted = this.IsDeleted,
                DeletedAt = this.DeletedAt,
                BranchId = this.BranchId
            };
        }

        public AccountInfo UpdatePasswordChange(IDateTimeService dateTimeService)
        {
            if (dateTimeService == null)
                throw new ArgumentNullException(nameof(dateTimeService));

            return new AccountInfo
            {
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                LastPasswordChangeAt = dateTimeService.UtcNow,
                LoginAttempts = this.LoginAttempts,
                IsActive = this.IsActive,
                IsDeleted = this.IsDeleted,
                DeletedAt = this.DeletedAt,
                BranchId = this.BranchId
            };
        }

        public AccountInfo IncrementLoginAttempts()
        {
            return new AccountInfo
            {
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                LastPasswordChangeAt = this.LastPasswordChangeAt,
                LoginAttempts = this.LoginAttempts + 1,
                IsActive = this.IsActive,
                IsDeleted = this.IsDeleted,
                DeletedAt = this.DeletedAt,
                BranchId = this.BranchId
            };
        }

        public AccountInfo Deactivate()
        {
            return new AccountInfo
            {
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                LastPasswordChangeAt = this.LastPasswordChangeAt,
                LoginAttempts = this.LoginAttempts,
                IsActive = false,
                IsDeleted = this.IsDeleted,
                DeletedAt = this.DeletedAt,
                BranchId = this.BranchId
            };
        }

        public AccountInfo Activate()
        {
            return new AccountInfo
            {
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                LastPasswordChangeAt = this.LastPasswordChangeAt,
                LoginAttempts = this.LoginAttempts,
                IsActive = true,
                IsDeleted = this.IsDeleted,
                DeletedAt = this.DeletedAt,
                BranchId = this.BranchId
            };
        }

        public AccountInfo MarkAsDeleted(IDateTimeService dateTimeService)
        {
            if (dateTimeService == null)
                throw new ArgumentNullException(nameof(dateTimeService));

            return new AccountInfo
            {
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                LastPasswordChangeAt = this.LastPasswordChangeAt,
                LoginAttempts = this.LoginAttempts,
                IsActive = false,
                IsDeleted = true,
                DeletedAt = dateTimeService.UtcNow,
                BranchId = this.BranchId
            };
        }

        public bool IsLocked(int maxLoginAttempts = 5)
        {
            return LoginAttempts >= maxLoginAttempts;
        }

        bool IAccountInfo.IsLocked()
        {
            return IsLocked();
        }
    }
}
