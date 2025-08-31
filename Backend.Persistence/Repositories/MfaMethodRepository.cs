using Backend.Domain.Entities.MFA;
using Backend.Domain.Enums;
using Backend.Domain.Interfaces.Repositories;
using Backend.Domain.Specifications;
using Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Persistence.Repositories;

/// <summary>
/// Repository implementation for MFA methods
/// </summary>
public class MfaMethodRepository : BaseRepository<MfaMethod, Guid>, IMfaMethodRepository
{
    public MfaMethodRepository(
        ApplicationDbContext context, 
        ILogger<MfaMethodRepository> logger) 
        : base(context, logger)
    {
    }

    /// <summary>
    /// Get all MFA methods for a user
    /// </summary>
    public async Task<IEnumerable<MfaMethod>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting MFA methods for user {UserId}", userId);

            return await _dbSet
                .AsNoTracking()
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.Type)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA methods for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get enabled MFA methods for a user
    /// </summary>
    public async Task<IEnumerable<MfaMethod>> GetEnabledByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting enabled MFA methods for user {UserId}", userId);

            return await _dbSet
                .AsNoTracking()
                .Where(m => m.UserId == userId && m.IsEnabled)
                .OrderBy(m => m.Type)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enabled MFA methods for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get MFA method by user ID and type
    /// </summary>
    public async Task<MfaMethod?> GetByUserIdAndTypeAsync(string userId, MfaType type, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting MFA method for user {UserId}, type {Type}", userId, type);

            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId && m.Type == type, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA method for user {UserId}, type {Type}", userId, type);
            throw;
        }
    }

    /// <summary>
    /// Check if user has any enabled MFA methods
    /// </summary>
    public async Task<bool> HasEnabledMfaAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking if user {UserId} has enabled MFA", userId);

            return await _dbSet
                .AsNoTracking()
                .AnyAsync(m => m.UserId == userId && m.IsEnabled, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking enabled MFA for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get MFA methods by specification
    /// </summary>
    public async Task<IEnumerable<MfaMethod>> GetBySpecificationAsync(ISpecification<MfaMethod> specification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting MFA methods by specification");

            var query = _dbSet.AsNoTracking();
            
            // Apply criteria
            if (specification.Criteria != null)
                query = query.Where(specification.Criteria);

            // Apply includes
            foreach (var include in specification.Includes)
            {
                query = query.Include(include);
            }

            // Apply ordering
            if (specification.OrderBy != null)
                query = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null)
                query = query.OrderByDescending(specification.OrderByDescending);

            // Apply paging
            if (specification.IsPagingEnabled)
                query = query.Skip(specification.Skip).Take(specification.Take);

            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA methods by specification");
            throw;
        }
    }
} 