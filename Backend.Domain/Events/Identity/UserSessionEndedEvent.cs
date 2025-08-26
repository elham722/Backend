using System;

namespace Backend.Domain.Events.Identity
{
    public class UserSessionEndedEvent : BaseDomainEvent
    {
        public Guid SessionId { get; }
        public string UserId { get; }
        public string SessionToken { get; }
        public string? EndReason { get; }

        public UserSessionEndedEvent(Guid sessionId, string userId, string sessionToken, string? endReason)
        {
            SessionId = sessionId;
            UserId = userId;
            SessionToken = sessionToken;
            EndReason = endReason;
        }
    }
} 