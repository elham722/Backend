using System;

namespace Backend.Domain.Events.Identity
{
    public class PermissionDeactivatedEvent : BaseDomainEvent
    {
        public Guid PermissionId { get; }
        public string PermissionName { get; }

        public PermissionDeactivatedEvent(Guid permissionId, string permissionName)
        {
            PermissionId = permissionId;
            PermissionName = permissionName;
        }
    }
} 