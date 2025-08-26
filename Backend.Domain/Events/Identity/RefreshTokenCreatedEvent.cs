using System;

namespace Backend.Domain.Events.Identity
{
    public class RefreshTokenCreatedEvent : BaseDomainEvent
    {
        public Guid RefreshTokenId { get; }
        public string UserId { get; }

        public RefreshTokenCreatedEvent(Guid refreshTokenId, string userId)
        {
            RefreshTokenId = refreshTokenId;
            UserId = userId;
        }
    }
} 