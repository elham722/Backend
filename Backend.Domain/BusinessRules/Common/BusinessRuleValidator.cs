using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Domain.Exceptions.Common;

namespace Backend.Domain.BusinessRules.Common
{
    public static class BusinessRuleValidator
    {
        public static void Validate(params IBusinessRule[] businessRules)
        {
            var brokenRules = businessRules.Where(rule => rule.IsBroken()).ToList();
            
            if (brokenRules.Any())
            {
                var messages = brokenRules.Select(rule => rule.Message);
                throw new BusinessRuleViolationException(string.Join("; ", messages));
            }
        }

        public static void Validate(IEnumerable<IBusinessRule> businessRules)
        {
            Validate(businessRules.ToArray());
        }

        public static async Task ValidateAsync(params IBusinessRule[] businessRules)
        {
            var brokenRules = businessRules.Where(rule => rule.IsBroken()).ToList();
            
            if (brokenRules.Any())
            {
                var messages = brokenRules.Select(rule => rule.Message);
                throw new BusinessRuleViolationException(string.Join("; ", messages));
            }
            
            await Task.CompletedTask;
        }

        public static async Task ValidateAsync(IEnumerable<IBusinessRule> businessRules)
        {
            await ValidateAsync(businessRules.ToArray());
        }

        public static bool AreValid(params IBusinessRule[] businessRules)
        {
            return businessRules.All(rule => !rule.IsBroken());
        }

        public static bool AreValid(IEnumerable<IBusinessRule> businessRules)
        {
            return AreValid(businessRules.ToArray());
        }

        public static IEnumerable<string> GetBrokenRuleMessages(params IBusinessRule[] businessRules)
        {
            return businessRules.Where(rule => rule.IsBroken()).Select(rule => rule.Message);
        }

        public static IEnumerable<string> GetBrokenRuleMessages(IEnumerable<IBusinessRule> businessRules)
        {
            return GetBrokenRuleMessages(businessRules.ToArray());
        }
    }
} 