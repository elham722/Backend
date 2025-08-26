using System;

namespace Backend.Domain.Events.Identity
{
    public class PermissionNameChangedEvent : BaseDomainEvent
    {
        public Guid PermissionId { get; }
        public string OldName { get; }
        public string NewName { get; }

        public PermissionNameChangedEvent(Guid permissionId, string oldName, string newName)
        {
            PermissionId = permissionId;
            OldName = oldName;
            NewName = newName;
        }
    }
} 