using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Interfaces.Repositories;
using Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Persistence.Repositories;

/// <summary>
/// Base repository implementation providing common CRUD operations
/// </summary>
public abstract class BaseRepository<T, TId> : IGenericRepository<T, TId>
    where T : BaseAggregateRoot<TId>
    where TId : IEquatable<TId>
{
    protected readonly ApplicationDbContext _context;
    protected readonly ILogger<BaseRepository<T, TId>> _logger;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(ApplicationDbContext context, ILogger<BaseRepository<T, TId>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = context.Set<T>();
    }

    #region Read Operations

    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity by ID {Id}", id);
            throw;
        }
    }

    public virtual async Task<T?> GetByIdAsync(TId id, Expression<Func<T, object>>[] includes, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity by ID {Id} with includes", id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities");
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, object>>[] includes, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities with includes");
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entities with predicate");
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>>[] includes, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking().Where(predicate);
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entities with predicate and includes");
            throw;
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first entity with predicate");
            throw;
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>>[] includes, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking().Where(predicate);
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first entity with predicate and includes");
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(e => e.Id.Equals(id), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if entity exists with ID {Id}", id);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if entity exists with predicate");
            throw;
        }
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting entities");
            throw;
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting entities with predicate");
            throw;
        }
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var totalCount = await _dbSet
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var items = await _dbSet
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged entities");
            throw;
        }
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking().Where(predicate);
            
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged entities with predicate");
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetOrderedAsync(
        Expression<Func<T, object>> orderBy,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking();
            
            query = ascending 
                ? query.OrderBy(orderBy) 
                : query.OrderByDescending(orderBy);

            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ordered entities");
            throw;
        }
    }

    #endregion

    #region Write Operations

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding entity");
            throw;
        }
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding range of entities");
            throw;
        }
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity");
            throw;
        }
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.UpdateRange(entities);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating range of entities");
            throw;
        }
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity");
            throw;
        }
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting range of entities");
            throw;
        }
    }

    public virtual async Task DeleteRangeAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
            _dbSet.RemoveRange(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entities with predicate");
            throw;
        }
    }

    public virtual async Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return entities.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk inserting entities");
            throw;
        }
    }

    public virtual async Task<int> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.UpdateRange(entities);
            await Task.CompletedTask;
            return entities.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating entities");
            throw;
        }
    }

    public virtual async Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
            _dbSet.RemoveRange(entities);
            return entities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting entities");
            throw;
        }
    }

    #endregion

    #region Queryable Support

    /// <summary>
    /// Gets IQueryable for complex queries
    /// </summary>
    public virtual IQueryable<T> GetQueryable()
    {
        return _dbSet.AsNoTracking();
    }

    /// <summary>
    /// Gets IQueryable with tracking for updates
    /// </summary>
    public virtual IQueryable<T> GetQueryableWithTracking()
    {
        return _dbSet;
    }

    #endregion
} 