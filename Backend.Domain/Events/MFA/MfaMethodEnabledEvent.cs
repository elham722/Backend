using Backend.Domain.Events;
using Backend.Domain.Enums;

namespace Backend.Domain.Events.MFA;

/// <summary>
/// Event raised when an MFA method is enabled
/// </summary>
public class MfaMethodEnabledEvent : BaseDomainEvent
{
    public string UserId { get; }
    public MfaType MfaType { get; }
    public DateTime EnabledAt { get; }

    public MfaMethodEnabledEvent(string userId, MfaType mfaType) : base(DateTime.UtcNow)
    {
        UserId = userId;
        MfaType = mfaType;
        EnabledAt = DateTime.UtcNow;
    }
} 