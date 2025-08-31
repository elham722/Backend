using Backend.Domain.ValueObjects.Common;

namespace Backend.Domain.ValueObjects.Common;

/// <summary>
/// Value object representing audit information for entities
/// </summary>
public class AuditInfo : BaseValueObject
{
    public string? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private AuditInfo() { } // For EF Core

    public AuditInfo(
        string? createdBy = null,
        DateTime? createdAt = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        CreatedBy = createdBy;
        CreatedAt = createdAt ?? DateTime.UtcNow;
        ModifiedBy = null;
        ModifiedAt = null;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public AuditInfo UpdateModified(string? modifiedBy, string? ipAddress = null, string? userAgent = null)
    {
        return new AuditInfo
        {
            CreatedBy = this.CreatedBy,
            CreatedAt = this.CreatedAt,
            ModifiedBy = modifiedBy,
            ModifiedAt = DateTime.UtcNow,
            IpAddress = ipAddress ?? this.IpAddress,
            UserAgent = userAgent ?? this.UserAgent
        };
    }

    public bool HasBeenModified => ModifiedAt.HasValue;

    public bool WasCreatedBy(string userId)
    {
        return !string.IsNullOrEmpty(CreatedBy) && CreatedBy.Equals(userId, StringComparison.OrdinalIgnoreCase);
    }

    public bool WasLastModifiedBy(string userId)
    {
        return !string.IsNullOrEmpty(ModifiedBy) && ModifiedBy.Equals(userId, StringComparison.OrdinalIgnoreCase);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CreatedBy ?? string.Empty;
        yield return CreatedAt;
        yield return ModifiedBy ?? string.Empty;
        yield return ModifiedAt ?? DateTime.MinValue;
        yield return IpAddress ?? string.Empty;
        yield return UserAgent ?? string.Empty;
    }
} 