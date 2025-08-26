using System;

namespace Backend.Domain.Events.Identity
{
    public class UserSessionExtendedEvent : BaseDomainEvent
    {
        public Guid SessionId { get; }
        public string UserId { get; }
        public string SessionToken { get; }
        public DateTime? OldExpiresAt { get; }
        public DateTime NewExpiresAt { get; }

        public UserSessionExtendedEvent(Guid sessionId, string userId, string sessionToken, DateTime? oldExpiresAt, DateTime newExpiresAt)
        {
            SessionId = sessionId;
            UserId = userId;
            SessionToken = sessionToken;
            OldExpiresAt = oldExpiresAt;
            NewExpiresAt = newExpiresAt;
        }
    }
} 