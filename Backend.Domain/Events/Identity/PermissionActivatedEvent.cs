using System;

namespace Backend.Domain.Events.Identity
{
    public class PermissionActivatedEvent : BaseDomainEvent
    {
        public Guid PermissionId { get; }
        public string PermissionName { get; }

        public PermissionActivatedEvent(Guid permissionId, string permissionName)
        {
            PermissionId = permissionId;
            PermissionName = permissionName;
        }
    }
} 