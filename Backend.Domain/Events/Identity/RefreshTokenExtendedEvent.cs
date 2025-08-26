using System;

namespace Backend.Domain.Events.Identity
{
    public class RefreshTokenExtendedEvent : BaseDomainEvent
    {
        public Guid RefreshTokenId { get; }
        public string UserId { get; }
        public DateTime OldExpiresAt { get; }
        public DateTime NewExpiresAt { get; }

        public RefreshTokenExtendedEvent(Guid refreshTokenId, string userId, DateTime oldExpiresAt, DateTime newExpiresAt)
        {
            RefreshTokenId = refreshTokenId;
            UserId = userId;
            OldExpiresAt = oldExpiresAt;
            NewExpiresAt = newExpiresAt;
        }
    }
} 