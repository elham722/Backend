using Backend.Domain.BusinessRules.Common;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Domain.BusinessRules.Customer
{
    public class CustomerMustBeVerifiedForPremiumRule : BaseBusinessRule
    {
        private readonly Domain.Entities.Customer _customer;

        public CustomerMustBeVerifiedForPremiumRule(Domain.Entities.Customer customer)
        {
            _customer = customer ?? throw new ArgumentNullException(nameof(customer));
        }

        public override bool IsBroken()
        {
            return _customer.CustomerStatus != CustomerStatus.Verified;
        }

        public override string Message => $"Customer must be verified before upgrading to premium. Current status: {_customer.CustomerStatus}";
    }
} 