using Backend.Domain.Exceptions.Common;

namespace Backend.Domain.BusinessRules.Common
{
    public abstract class BaseBusinessRule : IBusinessRule
    {
        public abstract bool IsBroken();
        public abstract string Message { get; }

        public bool IsSatisfied() => !IsBroken();

        public virtual void Validate()
        {
            if (IsBroken())
                throw new BusinessRuleViolationException(Message);
        }

        public virtual async Task ValidateAsync()
        {
            if (IsBroken())
                throw new BusinessRuleViolationException(Message);
            await Task.CompletedTask;
        }

        protected void ThrowIfBroken()
        {
            if (IsBroken())
                throw new BusinessRuleViolationException(Message);
        }

        protected async Task ThrowIfBrokenAsync()
        {
            if (IsBroken())
                throw new BusinessRuleViolationException(Message);
            await Task.CompletedTask;
        }
    }
} 