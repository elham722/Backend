using System;

namespace Backend.Identity.ValueObjects
{
    public class AccountInfo
    {
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public DateTime? LastPasswordChangeAt { get; private set; }
        public int LoginAttempts { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        private AccountInfo() { } // For EF Core

        public static AccountInfo Create()
        {
            return new AccountInfo
            {
                CreatedAt = DateTime.UtcNow,
                LoginAttempts = 0,
                IsActive = true,
                IsDeleted = false
            };
        }

        public AccountInfo UpdateLastLogin()
        {
            return new AccountInfo
            {
                CreatedAt = this.CreatedAt,
                LastLoginAt = DateTime.UtcNow,
                LastPasswordChangeAt = this.LastPasswordChangeAt,
                LoginAttempts = 0, // Reset on successful login
                IsActive = this.IsActive,
                IsDeleted = this.IsDeleted,
                DeletedAt = this.DeletedAt
            };
        }

        public AccountInfo UpdatePasswordChange()
        {
            return new AccountInfo
            {
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                LastPasswordChangeAt = DateTime.UtcNow,
                LoginAttempts = this.LoginAttempts,
                IsActive = this.IsActive,
                IsDeleted = this.IsDeleted,
                DeletedAt = this.DeletedAt
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
                DeletedAt = this.DeletedAt
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
                DeletedAt = this.DeletedAt
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
                DeletedAt = this.DeletedAt
            };
        }

        public AccountInfo MarkAsDeleted()
        {
            return new AccountInfo
            {
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                LastPasswordChangeAt = this.LastPasswordChangeAt,
                LoginAttempts = this.LoginAttempts,
                IsActive = false,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            };
        }

        public bool IsLocked(int maxLoginAttempts = 5)
        {
            return LoginAttempts >= maxLoginAttempts;
        }
    }
}
