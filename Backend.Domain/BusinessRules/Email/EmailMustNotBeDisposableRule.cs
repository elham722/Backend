using Backend.Domain.BusinessRules.Common;
using Backend.Domain.ValueObjects;

namespace Backend.Domain.BusinessRules.Email
{
    public class EmailMustNotBeDisposableRule : BaseBusinessRule
    {
        private readonly ValueObjects.Email _email;
        private readonly string _operation;

        public EmailMustNotBeDisposableRule(ValueObjects.Email email, string operation = "operation")
        {
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _operation = operation;
        }

        public override bool IsBroken()
        {
            return _email.IsDisposableEmail();
        }

        public override string Message => $"Cannot use disposable email for {_operation}. Email domain: {_email.GetDomain()}";
    }
} 