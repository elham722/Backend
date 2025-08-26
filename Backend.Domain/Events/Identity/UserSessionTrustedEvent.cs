using System;

namespace Backend.Domain.Events.Identity
{
    public class UserSessionTrustedEvent : BaseDomainEvent
    {
        public Guid SessionId { get; }
        public string UserId { get; }
        public string SessionToken { get; }
        public string? TrustReason { get; }

        public UserSessionTrustedEvent(Guid sessionId, string userId, string sessionToken, string? trustReason)
        {
            SessionId = sessionId;
            UserId = userId;
            SessionToken = sessionToken;
            TrustReason = trustReason;
        }
    }
} 