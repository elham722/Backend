using System;

namespace Backend.Domain.Exceptions.Common
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string name, object key)
            : base($"{name} with key {key} was not found.")
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 