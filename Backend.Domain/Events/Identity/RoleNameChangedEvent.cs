using System;

namespace Backend.Domain.Events.Identity
{
    public class RoleNameChangedEvent : BaseDomainEvent
    {
        public Guid RoleId { get; }
        public string OldName { get; }
        public string NewName { get; }

        public RoleNameChangedEvent(Guid roleId, string oldName, string newName)
        {
            RoleId = roleId;
            OldName = oldName;
            NewName = newName;
        }
    }
} 