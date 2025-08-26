using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Domain.ValueObjects.Common 
{
    /// <summary>
    /// Base class for Value Objects in DDD, providing equality comparison based on components.
    /// </summary>
    public abstract class BaseValueObject
    {
        /// <summary>
        /// Gets the components to compare for equality.
        /// </summary>
        protected abstract IEnumerable<object?> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var other = (BaseValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(17, (current, obj) => current * 23 ^ (obj?.GetHashCode() ?? 0));
        }

        public static bool operator ==(BaseValueObject? left, BaseValueObject? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseValueObject? left, BaseValueObject? right)
        {
            return !Equals(left, right);
        }
    }
}