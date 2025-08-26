using System;

namespace Backend.Domain.Events.Identity
{
    public class PermissionCreatedEvent : BaseDomainEvent
    {
        public Guid PermissionId { get; }
        public string PermissionName { get; }

        public PermissionCreatedEvent(Guid permissionId, string permissionName)
        {
            PermissionId = permissionId;
            PermissionName = permissionName;
        }
    }
} 