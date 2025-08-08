using Backend.Domain.BusinessRules.Common;
using Backend.Domain.Entities;

namespace Backend.Domain.BusinessRules.Customer
{
    public class CustomerMustHaveValidContactInfoRule : BaseBusinessRule
    {
        private readonly Domain.Entities.Customer _customer;
        private readonly string _operation;

        public CustomerMustHaveValidContactInfoRule(Domain.Entities.Customer customer, string operation = "operation")
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _operation = operation;
        }

        public override bool IsBroken()
        {
            return !_customer.HasValidContactInfo();
        }

        public override string Message => $"Customer must have valid contact information (email and phone/mobile) to perform {_operation}";
    }
} 