using System;

namespace Backend.Domain.Events.Identity
{
    public class UserSessionDeactivatedEvent : BaseDomainEvent
    {
        public Guid SessionId { get; }
        public string UserId { get; }
        public string SessionToken { get; }
        public string? Reason { get; }

        public UserSessionDeactivatedEvent(Guid sessionId, string userId, string sessionToken, string? reason)
        {
            SessionId = sessionId;
            UserId = userId;
            SessionToken = sessionToken;
            Reason = reason;
        }
    }
} 