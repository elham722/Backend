using System;

namespace Backend.Domain.Events.Identity
{
    public class UserSessionStartedEvent : BaseDomainEvent
    {
        public Guid SessionId { get; }
        public string UserId { get; }
        public string SessionToken { get; }

        public UserSessionStartedEvent(Guid sessionId, string userId, string sessionToken)
        {
            SessionId = sessionId;
            UserId = userId;
            SessionToken = sessionToken;
        }
    }
} 