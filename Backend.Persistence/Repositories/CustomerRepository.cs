using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.Interfaces.Repositories;
using Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Persistence.Repositories;

/// <summary>
/// Repository implementation for Customer entity
/// </summary>
public class CustomerRepository : BaseRepository<Customer, Guid>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context, ILogger<CustomerRepository> logger) 
        : base(context, logger)
    {
    }

    public async Task<Customer?> GetByApplicationUserIdAsync(string applicationUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by application user ID {ApplicationUserId}", applicationUserId);
            throw;
        }
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email.Value == email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by email {Email}", email);
            throw;
        }
    }

    public async Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.PhoneNumber.Value == phone, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by phone {Phone}", phone);
            throw;
        }
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.CustomerStatus == status)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers by status {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.CustomerStatus == CustomerStatus.Active)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active customers");
            throw;
        }
    }

    public async Task<IEnumerable<Customer>> GetCustomersByRegistrationDateAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers by registration date range {FromDate} to {ToDate}", fromDate, toDate);
            throw;
        }
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return !await _dbSet
                .AsNoTracking()
                .AnyAsync(c => c.Email.Value == email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if email is unique {Email}", email);
            throw;
        }
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, CancellationToken cancellationToken = default)
    {
        try
        {
            return !await _dbSet
                .AsNoTracking()
                .AnyAsync(c => c.PhoneNumber.Value == phone, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if phone is unique {Phone}", phone);
            throw;
        }
    }

    public async Task<int> GetCustomerCountByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(c => c.CustomerStatus == status, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer count by status {Status}", status);
            throw;
        }
    }

    #region Additional Business-Specific Methods

    /// <summary>
    /// Gets customers by age range
    /// </summary>
    public async Task<IEnumerable<Customer>> GetCustomersByAgeRangeAsync(int minAge, int maxAge, CancellationToken cancellationToken = default)
    {
        try
        {
            var minDate = DateTime.UtcNow.AddYears(-maxAge);
            var maxDate = DateTime.UtcNow.AddYears(-minAge);

            return await _dbSet
                .AsNoTracking()
                .Where(c => c.DateOfBirth >= minDate && c.DateOfBirth <= maxDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers by age range {MinAge} to {MaxAge}", minAge, maxAge);
            throw;
        }
    }

    /// <summary>
    /// Gets customers by location (city)
    /// </summary>
    public async Task<IEnumerable<Customer>> GetCustomersByLocationAsync(string city, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.PrimaryAddress.City == city)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers by location {City}", city);
            throw;
        }
    }

    /// <summary>
    /// Gets premium customers
    /// </summary>
    public async Task<IEnumerable<Customer>> GetPremiumCustomersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsPremium && c.CustomerStatus == CustomerStatus.Active)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting premium customers");
            throw;
        }
    }

    /// <summary>
    /// Gets verified customers
    /// </summary>
    public async Task<IEnumerable<Customer>> GetVerifiedCustomersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IsVerified && c.CustomerStatus == CustomerStatus.Active)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verified customers");
            throw;
        }
    }

    /// <summary>
    /// Gets customers with incomplete profiles
    /// </summary>
    public async Task<IEnumerable<Customer>> GetCustomersWithIncompleteProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => string.IsNullOrEmpty(c.Email.Value) || 
                           string.IsNullOrEmpty(c.PhoneNumber.Value) ||
                           string.IsNullOrEmpty(c.PrimaryAddress.City))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers with incomplete profiles");
            throw;
        }
    }

    /// <summary>
    /// Searches customers by term (name, email, phone)
    /// </summary>
    public async Task<IEnumerable<Customer>> GetCustomersBySearchTermAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var term = searchTerm.ToLower();
            
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.FirstName.ToLower().Contains(term) ||
                           c.LastName.ToLower().Contains(term) ||
                           c.Email.Value.ToLower().Contains(term) ||
                           c.PhoneNumber.Value.Contains(term))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers by term {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Gets premium customer count
    /// </summary>
    public async Task<int> GetPremiumCustomerCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(c => c.IsPremium && c.CustomerStatus == CustomerStatus.Active, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting premium customer count");
            throw;
        }
    }

    /// <summary>
    /// Gets verified customer count
    /// </summary>
    public async Task<int> GetVerifiedCustomerCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .CountAsync(c => c.IsVerified && c.CustomerStatus == CustomerStatus.Active, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verified customer count");
            throw;
        }
    }

    #endregion
} 