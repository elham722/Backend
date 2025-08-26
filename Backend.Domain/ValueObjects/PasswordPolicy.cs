using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Backend.Domain.ValueObjects.Common;
using Backend.Domain.Common;

namespace Backend.Domain.ValueObjects
{
    public class PasswordPolicy : BaseValueObject
    {
        public int MinLength { get; private set; }
        public int MaxLength { get; private set; }
        public bool RequireUppercase { get; private set; }
        public bool RequireLowercase { get; private set; }
        public bool RequireDigit { get; private set; }
        public bool RequireSpecialCharacter { get; private set; }
        public int MinSpecialCharacters { get; private set; }
        public int MinDigits { get; private set; }
        public int MaxConsecutiveCharacters { get; private set; }
        public int PasswordHistoryCount { get; private set; }
        public int ExpirationDays { get; private set; }
        public bool PreventCommonPasswords { get; private set; }

        private PasswordPolicy() { } // For EF Core

        public PasswordPolicy(int minLength = 8, int maxLength = 128, bool requireUppercase = true, 
            bool requireLowercase = true, bool requireDigit = true, bool requireSpecialCharacter = true,
            int minSpecialCharacters = 1, int minDigits = 1, int maxConsecutiveCharacters = 3,
            int passwordHistoryCount = 5, int expirationDays = 90, bool preventCommonPasswords = true)
        {
            Guard.AgainstNegative(minLength, nameof(minLength));
            Guard.AgainstNegative(maxLength, nameof(maxLength));
            Guard.AgainstNegative(minSpecialCharacters, nameof(minSpecialCharacters));
            Guard.AgainstNegative(minDigits, nameof(minDigits));
            Guard.AgainstNegative(maxConsecutiveCharacters, nameof(maxConsecutiveCharacters));
            Guard.AgainstNegative(passwordHistoryCount, nameof(passwordHistoryCount));
            Guard.AgainstNegative(expirationDays, nameof(expirationDays));

            if (minLength > maxLength)
                throw new ArgumentException("Minimum length cannot be greater than maximum length");

            if (minLength < 6)
                throw new ArgumentException("Minimum length cannot be less than 6");

            if (maxLength > 256)
                throw new ArgumentException("Maximum length cannot be greater than 256");

            MinLength = minLength;
            MaxLength = maxLength;
            RequireUppercase = requireUppercase;
            RequireLowercase = requireLowercase;
            RequireDigit = requireDigit;
            RequireSpecialCharacter = requireSpecialCharacter;
            MinSpecialCharacters = minSpecialCharacters;
            MinDigits = minDigits;
            MaxConsecutiveCharacters = maxConsecutiveCharacters;
            PasswordHistoryCount = passwordHistoryCount;
            ExpirationDays = expirationDays;
            PreventCommonPasswords = preventCommonPasswords;
        }

        public static PasswordPolicy CreateDefault()
        {
            return new PasswordPolicy();
        }

        public static PasswordPolicy CreateStrong()
        {
            return new PasswordPolicy(
                minLength: 12,
                maxLength: 128,
                requireUppercase: true,
                requireLowercase: true,
                requireDigit: true,
                requireSpecialCharacter: true,
                minSpecialCharacters: 2,
                minDigits: 2,
                maxConsecutiveCharacters: 2,
                passwordHistoryCount: 10,
                expirationDays: 60,
                preventCommonPasswords: true
            );
        }

        public static PasswordPolicy CreateWeak()
        {
            return new PasswordPolicy(
                minLength: 6,
                maxLength: 128,
                requireUppercase: false,
                requireLowercase: true,
                requireDigit: true,
                requireSpecialCharacter: false,
                minSpecialCharacters: 0,
                minDigits: 1,
                maxConsecutiveCharacters: 5,
                passwordHistoryCount: 3,
                expirationDays: 180,
                preventCommonPasswords: false
            );
        }

        // Validation Methods
        public bool IsValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < MinLength || password.Length > MaxLength)
                return false;

            if (RequireUppercase && !password.Any(char.IsUpper))
                return false;

            if (RequireLowercase && !password.Any(char.IsLower))
                return false;

            if (RequireDigit && password.Count(char.IsDigit) < MinDigits)
                return false;

            if (RequireSpecialCharacter && password.Count(IsSpecialCharacter) < MinSpecialCharacters)
                return false;

            if (HasConsecutiveCharacters(password))
                return false;

            if (PreventCommonPasswords && IsCommonPassword(password))
                return false;

            return true;
        }

        public List<string> GetValidationErrors(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password cannot be empty");
                return errors;
            }

            if (password.Length < MinLength)
                errors.Add($"Password must be at least {MinLength} characters long");

            if (password.Length > MaxLength)
                errors.Add($"Password cannot exceed {MaxLength} characters");

            if (RequireUppercase && !password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter");

            if (RequireLowercase && !password.Any(char.IsLower))
                errors.Add("Password must contain at least one lowercase letter");

            if (RequireDigit && password.Count(char.IsDigit) < MinDigits)
                errors.Add($"Password must contain at least {MinDigits} digit(s)");

            if (RequireSpecialCharacter && password.Count(IsSpecialCharacter) < MinSpecialCharacters)
                errors.Add($"Password must contain at least {MinSpecialCharacters} special character(s)");

            if (HasConsecutiveCharacters(password))
                errors.Add($"Password cannot contain more than {MaxConsecutiveCharacters} consecutive characters");

            if (PreventCommonPasswords && IsCommonPassword(password))
                errors.Add("Password is too common, please choose a stronger password");

            return errors;
        }

        public int GetStrengthScore(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return 0;

            var score = 0;

            // Length score
            if (password.Length >= MinLength) score += 10;
            if (password.Length >= MinLength + 4) score += 10;
            if (password.Length >= MinLength + 8) score += 10;

            // Character variety score
            if (password.Any(char.IsUpper)) score += 10;
            if (password.Any(char.IsLower)) score += 10;
            if (password.Any(char.IsDigit)) score += 10;
            if (password.Any(IsSpecialCharacter)) score += 10;

            // Complexity score
            var uniqueChars = password.Distinct().Count();
            if (uniqueChars >= password.Length * 0.7) score += 10;
            if (uniqueChars >= password.Length * 0.8) score += 10;

            // Bonus for meeting all requirements
            if (IsValid(password)) score += 20;

            return Math.Min(score, 100);
        }

        public string GetStrengthDescription(string password)
        {
            var score = GetStrengthScore(password);

            return score switch
            {
                >= 80 => "Very Strong",
                >= 60 => "Strong",
                >= 40 => "Medium",
                >= 20 => "Weak",
                _ => "Very Weak"
            };
        }

        // Helper Methods
        private bool IsSpecialCharacter(char c)
        {
            return !char.IsLetterOrDigit(c);
        }

        private bool HasConsecutiveCharacters(string password)
        {
            if (MaxConsecutiveCharacters <= 1)
                return false;

            for (int i = 0; i <= password.Length - MaxConsecutiveCharacters; i++)
            {
                var consecutive = true;
                for (int j = 1; j < MaxConsecutiveCharacters; j++)
                {
                    if (password[i] != password[i + j])
                    {
                        consecutive = false;
                        break;
                    }
                }
                if (consecutive)
                    return true;
            }
            return false;
        }

        private bool IsCommonPassword(string password)
        {
            var commonPasswords = new[]
            {
                "password", "123456", "123456789", "qwerty", "abc123", "password123",
                "admin", "letmein", "welcome", "monkey", "dragon", "master", "football"
            };

            return commonPasswords.Contains(password.ToLowerInvariant());
        }

        // Override methods
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return MinLength;
            yield return MaxLength;
            yield return RequireUppercase;
            yield return RequireLowercase;
            yield return RequireDigit;
            yield return RequireSpecialCharacter;
            yield return MinSpecialCharacters;
            yield return MinDigits;
            yield return MaxConsecutiveCharacters;
            yield return PasswordHistoryCount;
            yield return ExpirationDays;
            yield return PreventCommonPasswords;
        }

        public override string ToString()
        {
            return $"PasswordPolicy(Min:{MinLength}, Max:{MaxLength}, Uppercase:{RequireUppercase}, Lowercase:{RequireLowercase}, Digit:{RequireDigit}, Special:{RequireSpecialCharacter})";
        }
    }
} 