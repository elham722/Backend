using Backend.Domain.BusinessRules.Common;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Domain.BusinessRules.Customer
{
    public class CustomerMustBeActiveRule : BaseBusinessRule
    {
        private readonly Domain.Entities.Customer _customer;
        private readonly string _operation;

        public CustomerMustBeActiveRule(Domain.Entities.Customer customer, string operation = "operation")
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _operation = operation;
        }

        public override bool IsBroken()
        {
            return !_customer.IsActive;
        }

        public override string Message => $"Customer must be active to perform {_operation}, Customer status: {_customer.CustomerStatus}";
    }
} 