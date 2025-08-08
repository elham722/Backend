namespace Backend.Domain.BusinessRules.Common
{
    public interface IBusinessRule
    {
        bool IsBroken();
        string Message { get; }
    }
} 