using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Domain.ValueObjects.Common
{
    public abstract class BaseValueObject
    {
        protected abstract IEnumerable<object?> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            var other = (BaseValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked { return current * 23 + (obj?.GetHashCode() ?? 0); }
                });
        }

        public static bool operator ==(BaseValueObject? left, BaseValueObject? right)
        {
            return EqualityComparer<BaseValueObject>.Default.Equals(left, right);
        }

        public static bool operator !=(BaseValueObject? left, BaseValueObject? right)
        {
            return !(left == right);
        }
    }
}
