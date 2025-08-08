using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Domain.Aggregates.Common;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Domain.Interfaces.Repositories
{
    public interface ICustomerRepository : IGenericRepository<Customer, Guid>
    {
        // Customer-specific query methods
        Task<Customer?> GetByApplicationUserIdAsync(string applicationUserId, CancellationToken cancellationToken = default);
        Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
        Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Customer>> GetCustomersByRegistrationDateAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        
        // Customer-specific business methods
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsPhoneUniqueAsync(string phone, CancellationToken cancellationToken = default);
        Task<int> GetCustomerCountByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default);
    }
} 