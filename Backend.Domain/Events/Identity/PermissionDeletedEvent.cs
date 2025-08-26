using System;

namespace Backend.Domain.Events.Identity
{
    public class PermissionDeletedEvent : BaseDomainEvent
    {
        public Guid PermissionId { get; }
        public string PermissionName { get; }

        public PermissionDeletedEvent(Guid permissionId, string permissionName)
        {
            PermissionId = permissionId;
            PermissionName = permissionName;
        }
    }
} 