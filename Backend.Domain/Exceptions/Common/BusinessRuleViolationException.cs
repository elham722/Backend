using System;

namespace Backend.Domain.Exceptions.Common
{
    public class BusinessRuleViolationException : DomainException
    {
        public BusinessRuleViolationException(string message) : base(message)
        {
        }

        public BusinessRuleViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 