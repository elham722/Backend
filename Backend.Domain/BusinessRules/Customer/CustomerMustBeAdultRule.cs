using Backend.Domain.BusinessRules.Common;
using Backend.Domain.Entities;

namespace Backend.Domain.BusinessRules.Customer
{
    public class CustomerMustBeAdultRule : BaseBusinessRule
    {
        private readonly Domain.Entities.Customer _customer;
        private readonly string _operation;

        public CustomerMustBeAdultRule(Domain.Entities.Customer customer, string operation = "operation")
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _operation = operation;
        }

        public override bool IsBroken()
        {
            return !_customer.IsAdult;
        }

        public override string Message => $"Customer must be adult (18+) to perform {_operation}. Current age: {_customer.Age ?? 0}";
    }
} 