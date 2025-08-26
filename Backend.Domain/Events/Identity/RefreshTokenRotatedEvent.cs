using System;

namespace Backend.Domain.Events.Identity
{
    public class RefreshTokenRotatedEvent : BaseDomainEvent
    {
        public Guid OldRefreshTokenId { get; }
        public string UserId { get; }
        public Guid NewRefreshTokenId { get; }

        public RefreshTokenRotatedEvent(Guid oldRefreshTokenId, string userId, Guid newRefreshTokenId)
        {
            OldRefreshTokenId = oldRefreshTokenId;
            UserId = userId;
            NewRefreshTokenId = newRefreshTokenId;
        }
    }
} 