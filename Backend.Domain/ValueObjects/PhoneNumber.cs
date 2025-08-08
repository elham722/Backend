using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Backend.Domain.ValueObjects.Common;
using Backend.Domain.Common;

namespace Backend.Domain.ValueObjects
{
    public class PhoneNumber : BaseValueObject
    {
        public string Value { get; private set; } = null!;

        private PhoneNumber() { } // For EF Core

        public PhoneNumber(string value)
        {
            Guard.AgainstNullOrEmpty(value, nameof(value));

            if (!IsValidPhoneNumber(value))
                throw new ArgumentException("Invalid phone number format", nameof(value));

            Value = NormalizePhoneNumber(value);
        }

        public static PhoneNumber Create(string value)
        {
            return new PhoneNumber(value);
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            // فرمت‌های مختلف شماره تلفن ایران
            var patterns = new[]
            {
                @"^09\d{9}$", // موبایل
                @"^0\d{10}$", // ثابت
                @"^\+98\d{10}$", // بین‌المللی
                @"^0098\d{10}$" // بین‌المللی با 00
            };

            return patterns.Any(pattern => Regex.IsMatch(phoneNumber, pattern));
        }

        private static string NormalizePhoneNumber(string phoneNumber)
        {
            // حذف کاراکترهای غیرضروری
            var cleaned = Regex.Replace(phoneNumber, @"[^\d+]", "");

            // تبدیل به فرمت استاندارد
            if (cleaned.StartsWith("+98"))
                return cleaned;
            else if (cleaned.StartsWith("0098"))
                return "+98" + cleaned.Substring(4);
            else if (cleaned.StartsWith("09"))
                return "+98" + cleaned.Substring(1);
            else if (cleaned.StartsWith("0"))
                return "+98" + cleaned.Substring(1);
            else
                return "+98" + cleaned;
        }

        // Business Logic Methods
        public bool IsMobile()
        {
            // شماره موبایل ایران: +98 + 9 + 9 رقم
            return Value.Length == 13 && Value.StartsWith("+98") && Value[3] == '9';
        }

        public bool IsLandline()
        {
            return !IsMobile();
        }

        public string GetLocalFormat()
        {
            if (Value.StartsWith("+98"))
            {
                var number = Value.Substring(3);
                return "0" + number;
            }
            return Value;
        }

        public string GetInternationalFormat()
        {
            return Value;
        }

        public string GetDisplayFormat()
        {
            if (Value.StartsWith("+98"))
            {
                var number = Value.Substring(3);
                if (number.StartsWith("9"))
                {
                    // موبایل: 0912-345-6789
                    return $"+98 {number.Substring(0, 4)}-{number.Substring(4, 3)}-{number.Substring(7)}";
                }
                else
                {
                    // ثابت: 021-12345678
                    return $"+98 {number.Substring(0, 3)}-{number.Substring(3)}";
                }
            }
            return Value;
        }

        public string GetAreaCode()
        {
            if (Value.StartsWith("+98"))
            {
                var number = Value.Substring(3);
                if (number.StartsWith("9"))
                {
                    // موبایل: 0912 -> 912
                    return number.Substring(0, 4);
                }
                else
                {
                    // ثابت: 021 -> 21
                    return number.Substring(0, 3);
                }
            }
            return string.Empty;
        }

        public bool IsTehranNumber()
        {
            var areaCode = GetAreaCode();
            var tehranCodes = new[] { "21", "26", "28", "29", "31", "32", "33", "34", "35", "36", "37", "38", "39" };
            return tehranCodes.Contains(areaCode);
        }

        public bool IsSameAreaCode(PhoneNumber other)
        {
            return GetAreaCode() == other.GetAreaCode();
        }

        public PhoneNumber GetAlternativeFormat()
        {
            if (Value.StartsWith("+98"))
            {
                var number = Value.Substring(3);
                if (number.StartsWith("9"))
                {
                    // تبدیل موبایل به فرمت 0098
                    return new PhoneNumber("0098" + number);
                }
                else
                {
                    // تبدیل ثابت به فرمت 0098
                    return new PhoneNumber("0098" + number);
                }
            }
            return this;
        }

        // Override methods
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    }
} 