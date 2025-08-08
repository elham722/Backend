using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Backend.Domain.Common;
using Backend.Domain.ValueObjects.Common;

namespace Backend.Domain.ValueObjects
{
    public class Email : BaseValueObject
    {
        public string Value { get; private set; } = null!;

        private Email() { } // For EF Core

        public Email(string value)
        {
            Guard.AgainstNullOrEmpty(value, nameof(value));

            if (!IsValidEmail(value))
                throw new ArgumentException("Invalid email format", nameof(value));

            Value = value.Trim().ToLowerInvariant();
        }

        public static Email Create(string value)
        {
            return new Email(value);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        // Business Logic Methods
        public string GetDomain()
        {
            var parts = Value.Split('@');
            return parts.Length == 2 ? parts[1] : string.Empty;
        }

        public string GetUsername()
        {
            var parts = Value.Split('@');
            return parts.Length == 2 ? parts[0] : string.Empty;
        }

        public bool IsBusinessEmail()
        {
            var domain = GetDomain();
            var personalDomains = new[] { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "live.com", "msn.com" };
            return !personalDomains.Contains(domain.ToLowerInvariant());
        }

        public bool IsCorporateEmail()
        {
            var domain = GetDomain();
            var corporateKeywords = new[] { "corp", "company", "business", "enterprise", "inc", "ltd", "llc" };
            return corporateKeywords.Any(keyword => domain.ToLowerInvariant().Contains(keyword));
        }

        public bool IsEducationalEmail()
        {
            var domain = GetDomain();
            var educationalKeywords = new[] { "edu", "ac", "school", "university", "college", "institute" };
            return educationalKeywords.Any(keyword => domain.ToLowerInvariant().Contains(keyword));
        }

        public bool IsGovernmentEmail()
        {
            var domain = GetDomain();
            var governmentKeywords = new[] { "gov", "government", "state", "municipal", "city" };
            return governmentKeywords.Any(keyword => domain.ToLowerInvariant().Contains(keyword));
        }

        public Email GetDisposableEmail()
        {
            // برای تست و توسعه
            var username = GetUsername();
            return new Email($"{username}+disposable@{GetDomain()}");
        }

        public bool IsDisposableEmail()
        {
            var domain = GetDomain();
            var disposableDomains = new[] { "10minutemail.com", "guerrillamail.com", "tempmail.org" };
            return disposableDomains.Contains(domain.ToLowerInvariant());
        }

        // Override methods
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(Email email) => email.Value;
    }
} 