using Backend.Domain.BusinessRules.Common;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.BusinessRules.PhoneNumber
{
    public class PhoneNumberMustBeTehranRule : BaseBusinessRule
    {
        private readonly ValueObjects.PhoneNumber _phoneNumber;
        private readonly string _operation;

        public PhoneNumberMustBeTehranRule(ValueObjects.PhoneNumber phoneNumber, string operation = "local operation")
        {
            _phoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
            _operation = operation;
        }

        public override bool IsBroken()
        {
            return !_phoneNumber.IsTehranNumber();
        }

        public override string Message => $"Phone number must be from Tehran for {_operation}. Current number: {_phoneNumber.Value}, Area code: {_phoneNumber.GetAreaCode()}";
    }
} 