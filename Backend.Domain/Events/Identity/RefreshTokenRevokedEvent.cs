using System;

namespace Backend.Domain.Events.Identity
{
    public class RefreshTokenRevokedEvent : BaseDomainEvent
    {
        public Guid RefreshTokenId { get; }
        public string UserId { get; }
        public string? RevocationReason { get; }

        public RefreshTokenRevokedEvent(Guid refreshTokenId, string userId, string? revocationReason)
        {
            RefreshTokenId = refreshTokenId;
            UserId = userId;
            RevocationReason = revocationReason;
        }
    }
} 