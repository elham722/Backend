using Backend.Domain.BusinessRules.Common;
using Backend.Domain.BusinessRules.Customer;
using Backend.Domain.BusinessRules.Email;
using Backend.Domain.BusinessRules.PhoneNumber;
using Backend.Domain.Entities;
using Backend.Domain.ValueObjects;
using System.Collections.Generic;

namespace Backend.Domain.BusinessRules.Factories
{
    public static class CustomerBusinessRulesFactory
    {
        public static IEnumerable<IBusinessRule> CreateOrderPlacementRules(Domain.Entities.Customer customer)
        {
            return new List<IBusinessRule>
            {
                new CustomerMustBeActiveRule(customer, "place order"),
                new CustomerMustBeAdultRule(customer, "place order"),
                new CustomerMustHaveValidContactInfoRule(customer, "place order"),
                new CustomerMustNotBeDeletedRule(customer, "place order")
            };
        }

        public static IEnumerable<IBusinessRule> CreatePremiumUpgradeRules(Domain.Entities.Customer customer)
        {
            return new List<IBusinessRule>
            {
                new CustomerMustBeActiveRule(customer, "upgrade to premium"),
                new CustomerMustBeVerifiedForPremiumRule(customer),
                new CustomerMustNotBeDeletedRule(customer, "upgrade to premium")
            };
        }

        public static IEnumerable<IBusinessRule> CreateSMSNotificationRules(Domain.Entities.Customer customer)
        {
            var rules = new List<IBusinessRule>
            {
                new CustomerMustBeActiveRule(customer, "receive SMS"),
                new CustomerMustNotBeDeletedRule(customer, "receive SMS")
            };

            if (customer.MobileNumber != null)
            {
                rules.Add(new PhoneNumberMustBeMobileRule(customer.MobileNumber, "SMS notification"));
            }

            return rules;
        }

        public static IEnumerable<IBusinessRule> CreateEmailNotificationRules(Domain.Entities.Customer customer)
        {
            var rules = new List<IBusinessRule>
            {
                new CustomerMustBeActiveRule(customer, "receive email"),
                new CustomerMustNotBeDeletedRule(customer, "receive email")
            };

            if (customer.Email != null)
            {
                rules.Add(new EmailMustNotBeDisposableRule(customer.Email, "email notification"));
            }

            return rules;
        }

        public static IEnumerable<IBusinessRule> CreateBusinessAccountRules(Domain.Entities.Customer customer)
        {
            var rules = new List<IBusinessRule>
            {
                new CustomerMustBeActiveRule(customer, "business account operations"),
                new CustomerMustNotBeDeletedRule(customer, "business account operations")
            };

            if (customer.Email != null)
            {
                rules.Add(new EmailMustBeBusinessEmailRule(customer.Email, "business account"));
            }

            return rules;
        }

        public static IEnumerable<IBusinessRule> CreateLocalServiceRules(Domain.Entities.Customer customer)
        {
            var rules = new List<IBusinessRule>
            {
                new CustomerMustBeActiveRule(customer, "local service"),
                new CustomerMustNotBeDeletedRule(customer, "local service")
            };

            if (customer.MobileNumber != null)
            {
                rules.Add(new PhoneNumberMustBeTehranRule(customer.MobileNumber, "local service"));
            }
            else if (customer.PhoneNumber != null)
            {
                rules.Add(new PhoneNumberMustBeTehranRule(customer.PhoneNumber, "local service"));
            }

            return rules;
        }

        public static CompositeBusinessRule CreateCompositeOrderRules(Domain.Entities.Customer customer)
        {
            return new CompositeBusinessRule(
                "Order placement validation failed",
                CreateOrderPlacementRules(customer).ToArray()
            );
        }

        public static CompositeBusinessRule CreateCompositePremiumRules(Domain.Entities.Customer customer)
        {
            return new CompositeBusinessRule(
                "Premium upgrade validation failed",
                CreatePremiumUpgradeRules(customer).ToArray()
            );
        }
    }
} 