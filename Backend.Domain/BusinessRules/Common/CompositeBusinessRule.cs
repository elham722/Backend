using System.Collections.Generic;
using System.Linq;

namespace Backend.Domain.BusinessRules.Common
{
    public class CompositeBusinessRule : IBusinessRule
    {
        private readonly IEnumerable<IBusinessRule> _businessRules;
        private readonly string _message;

        public CompositeBusinessRule(IEnumerable<IBusinessRule> businessRules, string message = "One or more business rules are broken")
        {
            _businessRules = businessRules ?? Enumerable.Empty<IBusinessRule>();
            _message = message;
        }

        public CompositeBusinessRule(string message = "One or more business rules are broken", params IBusinessRule[] businessRules)
        {
            _businessRules = businessRules ?? Enumerable.Empty<IBusinessRule>();
            _message = message;
        }

        public bool IsBroken()
        {
            return _businessRules.Any(rule => rule.IsBroken());
        }

        public string Message
        {
            get
            {
                if (!IsBroken())
                    return string.Empty;

                var brokenRules = _businessRules.Where(rule => rule.IsBroken());
                var messages = brokenRules.Select(rule => rule.Message);
                return $"{_message}: {string.Join("; ", messages)}";
            }
        }

        public IEnumerable<IBusinessRule> GetBrokenRules()
        {
            return _businessRules.Where(rule => rule.IsBroken());
        }

        public IEnumerable<IBusinessRule> GetAllRules()
        {
            return _businessRules;
        }

        public int TotalRules => _businessRules.Count();
        public int BrokenRulesCount => _businessRules.Count(rule => rule.IsBroken());
        public bool HasBrokenRules => IsBroken();
    }
} 