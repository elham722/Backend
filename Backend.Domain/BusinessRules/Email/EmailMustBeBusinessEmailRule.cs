using Backend.Domain.BusinessRules.Common;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.BusinessRules.Email
{
    public class EmailMustBeBusinessEmailRule : BaseBusinessRule
    {
        private readonly ValueObjects.Email _email;
        private readonly string _operation;

        public EmailMustBeBusinessEmailRule(ValueObjects.Email email, string operation = "operation")
        {
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _operation = operation;
        }

        public override bool IsBroken()
        {
            return !_email.IsBusinessEmail();
        }

        public override string Message => $"Email must be a business email for {_operation}. Current email: {_email.Value}";
    }
} 