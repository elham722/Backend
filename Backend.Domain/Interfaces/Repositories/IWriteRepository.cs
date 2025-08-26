using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Backend.Domain.Aggregates.Common;

namespace Backend.Domain.Interfaces.Repositories
{
    public interface IWriteRepository<T, TId> where T : BaseAggregateRoot<TId> where TId : IEquatable<TId>
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        /// <summary>
        /// Bulk operations may require external libraries (e.g., EFCore.BulkExtensions).
        /// </summary>
        Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    }
} 