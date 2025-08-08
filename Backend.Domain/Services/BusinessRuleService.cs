using Backend.Domain.BusinessRules.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Domain.Services
{
    public class BusinessRuleService : IBusinessRuleService
    {
        public void Validate(params IBusinessRule[] businessRules)
        {
            BusinessRuleValidator.Validate(businessRules);
        }

        public void Validate(IEnumerable<IBusinessRule> businessRules)
        {
            BusinessRuleValidator.Validate(businessRules);
        }

        public async Task ValidateAsync(params IBusinessRule[] businessRules)
        {
            await BusinessRuleValidator.ValidateAsync(businessRules);
        }

        public async Task ValidateAsync(IEnumerable<IBusinessRule> businessRules)
        {
            await BusinessRuleValidator.ValidateAsync(businessRules);
        }

        public bool AreValid(params IBusinessRule[] businessRules)
        {
            return BusinessRuleValidator.AreValid(businessRules);
        }

        public bool AreValid(IEnumerable<IBusinessRule> businessRules)
        {
            return BusinessRuleValidator.AreValid(businessRules);
        }

        public IEnumerable<string> GetBrokenRuleMessages(params IBusinessRule[] businessRules)
        {
            return BusinessRuleValidator.GetBrokenRuleMessages(businessRules);
        }

        public IEnumerable<string> GetBrokenRuleMessages(IEnumerable<IBusinessRule> businessRules)
        {
            return BusinessRuleValidator.GetBrokenRuleMessages(businessRules);
        }
    }
} 