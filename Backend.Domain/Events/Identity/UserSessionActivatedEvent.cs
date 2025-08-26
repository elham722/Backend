using System;

namespace Backend.Domain.Events.Identity
{
    public class UserSessionActivatedEvent : BaseDomainEvent
    {
        public Guid SessionId { get; }
        public string UserId { get; }
        public string SessionToken { get; }

        public UserSessionActivatedEvent(Guid sessionId, string userId, string sessionToken)
        {
            SessionId = sessionId;
            UserId = userId;
            SessionToken = sessionToken;
        }
    }
} 