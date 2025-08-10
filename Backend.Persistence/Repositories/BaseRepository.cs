using Backend.Domain.Aggregates.Common;
using Backend.Domain.Interfaces.Repositories;
using Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Backend.Persistence.Repositories;

/// <summary>
/// Base repository implementation for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TId">Entity ID type</typeparam>
public abstract class BaseRepository<T, TId> : IReadRepository<T, TId>, IWriteRepository<T, TId>
    where T : BaseAggregateRoot<TId>
    where TId : struct
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    #region Read Operations

    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<T?> GetByIdAsync(TId id, string[] includes, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(string[] includes, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string[] includes, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, string[] includes, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id.Equals(id), cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);
        var items = await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.CountAsync(predicate, cancellationToken);
        var items = await _dbSet
            .Where(predicate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<IEnumerable<T>> GetOrderedAsync(
        Expression<Func<T, object>> orderBy, 
        bool ascending = true, 
        CancellationToken cancellationToken = default)
    {
        var query = ascending ? _dbSet.OrderBy(orderBy) : _dbSet.OrderByDescending(orderBy);
        return await query.ToListAsync(cancellationToken);
    }

    public virtual IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    #endregion

    #region Write Operations

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.UpdateRange(entities);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteRangeAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        _dbSet.RemoveRange(entities);
    }

    public virtual async Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities.Count();
    }

    public virtual async Task<int> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.UpdateRange(entities);
        await Task.CompletedTask;
        return entities.Count();
    }

    public virtual async Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        _dbSet.RemoveRange(entities);
        return entities.Count;
    }

    #endregion
} 