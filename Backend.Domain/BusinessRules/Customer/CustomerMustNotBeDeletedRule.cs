using Backend.Domain.BusinessRules.Common;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Domain.BusinessRules.Customer
{
    public class CustomerMustNotBeDeletedRule : BaseBusinessRule
    {
        private readonly Domain.Entities.Customer _customer;
        private readonly string _operation;

        public CustomerMustNotBeDeletedRule(Domain.Entities.Customer customer, string operation = "operation")
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
            _operation = operation;
        }

        public override bool IsBroken()
        {
            return  _customer.CustomerStatus == CustomerStatus.Deleted;
        }

        public override string Message => $"Cannot perform {_operation} on deleted customer, Customer status: {_customer.CustomerStatus}";
    }
} 