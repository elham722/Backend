using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain.Common
{
    public static class Guard
    {
        public static void AgainstNullOrEmpty(string? value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new DomainException($"{name} cannot be empty.");
        }

        public static void Against(bool condition, string message)
        {
            if (condition) throw new DomainException(message);
        }
    }
}
