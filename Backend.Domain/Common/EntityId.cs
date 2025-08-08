using System;
using System.Collections.Generic;

namespace Backend.Domain.Common
{
    public abstract class EntityId<T> : IEquatable<EntityId<T>>
    {
        public T Value { get; }

        protected EntityId(T value)
        {
            Value = value;
        }

        public bool Equals(EntityId<T>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as EntityId<T>);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(EntityId<T>? left, EntityId<T>? right)
        {
            return EqualityComparer<EntityId<T>>.Default.Equals(left, right);
        }

        public static bool operator !=(EntityId<T>? left, EntityId<T>? right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
} 