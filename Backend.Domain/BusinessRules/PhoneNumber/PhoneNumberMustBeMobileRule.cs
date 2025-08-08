using Backend.Domain.BusinessRules.Common;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.BusinessRules.PhoneNumber
{
    public class PhoneNumberMustBeMobileRule : BaseBusinessRule
    {
        private readonly ValueObjects.PhoneNumber _phoneNumber;
        private readonly string _operation;

        public PhoneNumberMustBeMobileRule(ValueObjects.PhoneNumber phoneNumber, string operation = "SMS operation")
        {
            _phoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
            _operation = operation;
        }

        public override bool IsBroken()
        {
            return !_phoneNumber.IsMobile();
        }

        public override string Message => $"Phone number must be mobile for {_operation}. Current number: {_phoneNumber.Value}";
    }
} 