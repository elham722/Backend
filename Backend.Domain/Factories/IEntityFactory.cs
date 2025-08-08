using System;

namespace Backend.Domain.Factories
{
    public interface IEntityFactory<TEntity, TId>
    {
        TEntity Create(TId id);
        TEntity Create(TId id, string createdBy);
    }
} 