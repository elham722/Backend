using Backend.Domain.Entities.MFA;
using Backend.Domain.Enums;
using Backend.Domain.Specifications;

namespace Backend.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for MFA methods
/// </summary>
public interface IMfaMethodRepository : IGenericRepository<MfaMethod, Guid>
{
    /// <summary>
    /// Get all MFA methods for a user
    /// </summary>
    Task<IEnumerable<MfaMethod>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get enabled MFA methods for a user
    /// </summary>
    Task<IEnumerable<MfaMethod>> GetEnabledByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get MFA method by user ID and type
    /// </summary>
    Task<MfaMethod?> GetByUserIdAndTypeAsync(string userId, MfaType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has any enabled MFA methods
    /// </summary>
    Task<bool> HasEnabledMfaAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get MFA methods by specification
    /// </summary>
    Task<IEnumerable<MfaMethod>> GetBySpecificationAsync(ISpecification<MfaMethod> specification, CancellationToken cancellationToken = default);
} 