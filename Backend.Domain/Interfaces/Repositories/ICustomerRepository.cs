using DigitekShop.Domain.Interfaces;

namespace Backend.Domain.Interfaces.Repositories
{
    public interface ICustomerRepository : IGenericRepository<Entities.Customer, Guid>
    {
        Task<Entities.Customer?> GetByApplicationUserIdAsync(string applicationUserId, CancellationToken ct = default);
    }
} 