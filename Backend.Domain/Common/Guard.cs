using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Exceptions.Common;

namespace Backend.Domain.Common
{
    public static class Guard
    {
        public static void AgainstNullOrEmpty(string? value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new DomainException($"{name} cannot be empty.");
        }

        public static void Against(bool condition, string message)
        {
            if (condition) throw new DomainException(message);
        }

        public static void AgainstNull<T>(T? value, string name) where T : class
        {
            if (value == null) throw new DomainException($"{name} cannot be null.");
        }

        public static void AgainstEmpty<T>(T value, string name) where T : struct
        {
            if (value.Equals(default(T))) throw new DomainException($"{name} cannot be empty.");
        }

        public static void AgainstEmpty(Guid value, string name)
        {
            if (value == Guid.Empty) throw new DomainException($"{name} cannot be empty.");
        }

        public static void AgainstNegative(int value, string name)
        {
            if (value < 0) throw new DomainException($"{name} cannot be negative.");
        }

        public static void AgainstNegative(double value, string name)
        {
            if (value < 0) throw new DomainException($"{name} cannot be negative.");
        }

        public static void AgainstNegative(TimeSpan value, string name)
        {
            if (value < TimeSpan.Zero) throw new DomainException($"{name} cannot be negative.");
        }

        public static void AgainstOutOfRange(int value, int min, int max, string name)
        {
            if (value < min || value > max) throw new DomainException($"{name} must be between {min} and {max}.");
        }

        public static void AgainstOutOfRange(double value, double min, double max, string name)
        {
            if (value < min || value > max) throw new DomainException($"{name} must be between {min} and {max}.");
        }

        public static void AgainstInvalidFormat(string value, string pattern, string name)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
                throw new DomainException($"{name} has invalid format.");
        }

        public static void AgainstInvalidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    throw new DomainException("Invalid email format.");
            }
            catch
            {
                throw new DomainException("Invalid email format.");
            }
        }

        public static void AgainstInvalidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10)
                throw new DomainException("Invalid phone number format.");
        }

        public static void AgainstInvalidDate(DateTime date, string name)
        {
            if (date == default(DateTime))
                throw new DomainException($"{name} cannot be default date.");
        }

        public static void AgainstFutureDate(DateTime date, string name)
        {
            if (date > DateTime.UtcNow)
                throw new DomainException($"{name} cannot be in the future.");
        }

        public static void AgainstPastDate(DateTime date, string name)
        {
            if (date < DateTime.UtcNow)
                throw new DomainException($"{name} cannot be in the past.");
        }
    }
}
