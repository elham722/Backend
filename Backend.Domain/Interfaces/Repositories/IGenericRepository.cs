using Backend.Domain.Common;

namespace DigitekShop.Domain.Interfaces
{
    public interface IGenericRepository<T, TId> where T : BaseAggregateRoot<TId>
    {
        Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);
        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task DeleteAsync(T entity, CancellationToken ct = default);

    }
} 