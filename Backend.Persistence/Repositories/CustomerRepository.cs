using Backend.Domain.Entities;
using Backend.Domain.Interfaces.Repositories;
using Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Backend.Persistence.Repositories;

/// <summary>
/// Customer repository implementation
/// </summary>
public class CustomerRepository : BaseRepository<Customer, Guid>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    #region Interface Implementation

    public async Task<Customer?> GetByApplicationUserIdAsync(string applicationUserId, CancellationToken cancellationToken = default)
    {
        // This method will be implemented when we add ApplicationUser support
        // For now, return null as ApplicationUser is not yet implemented
        return await Task.FromResult<Customer?>(null);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Email.Value == email, cancellationToken);
    }

    public async Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.PhoneNumber.Value == phone, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(Domain.Enums.CustomerStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CustomerStatus == status)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Status == Domain.Enums.EntityStatus.Active)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersByRegistrationDateAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(c => c.Email.Value == email, cancellationToken);
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(c => c.PhoneNumber.Value == phone, cancellationToken);
    }

    public async Task<int> GetCustomerCountByStatusAsync(Domain.Enums.CustomerStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(c => c.CustomerStatus == status, cancellationToken);
    }

    #endregion

    #region Additional Business Methods

    public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.PhoneNumber.Value == phoneNumber, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetPremiumCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsPremium)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetVerifiedCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsVerified)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersByAgeRangeAsync(int minAge, int maxAge, CancellationToken cancellationToken = default)
    {
        var minDate = DateTime.UtcNow.AddYears(-maxAge);
        var maxDate = DateTime.UtcNow.AddYears(-minAge);

        return await _dbSet
            .Where(c => c.DateOfBirth >= minDate && c.DateOfBirth <= maxDate)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersByLocationAsync(string country, string? city = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.PrimaryAddress.Country == country);

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(c => c.PrimaryAddress.City == city);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Email.Value == email, cancellationToken);
    }

    public async Task<bool> PhoneNumberExistsAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.PhoneNumber.Value == phoneNumber, cancellationToken);
    }

    public async Task<int> GetPremiumCustomerCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(c => c.IsPremium, cancellationToken);
    }

    public async Task<int> GetVerifiedCustomerCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(c => c.IsVerified, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersCreatedInDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersWithIncompleteProfileAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Email == null || c.PhoneNumber == null || c.PrimaryAddress == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetCustomersBySearchTermAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var searchTermLower = searchTerm.ToLower();

        return await _dbSet
            .Where(c => 
                c.FirstName.ToLower().Contains(searchTermLower) ||
                c.LastName.ToLower().Contains(searchTermLower) ||
                (c.Email != null && c.Email.Value.ToLower().Contains(searchTermLower)) ||
                (c.PhoneNumber != null && c.PhoneNumber.Value.ToLower().Contains(searchTermLower)))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    #endregion
} 