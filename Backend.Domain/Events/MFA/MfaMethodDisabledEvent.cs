using Backend.Domain.Events;
using Backend.Domain.Enums;

namespace Backend.Domain.Events.MFA;

/// <summary>
/// Event raised when an MFA method is disabled
/// </summary>
public class MfaMethodDisabledEvent : BaseDomainEvent
{
    public string UserId { get; }
    public MfaType MfaType { get; }
    public DateTime DisabledAt { get; }

    public MfaMethodDisabledEvent(string userId, MfaType mfaType) : base(DateTime.UtcNow)
    {
        UserId = userId;
        MfaType = mfaType;
        DisabledAt = DateTime.UtcNow;
    }
} 