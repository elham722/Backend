using System;

namespace Backend.Domain.Events.Identity
{
    public class RoleDeletedEvent : BaseDomainEvent
    {
        public Guid RoleId { get; }
        public string RoleName { get; }

        public RoleDeletedEvent(Guid roleId, string roleName)
        {
            RoleId = roleId;
            RoleName = roleName;
        }
    }
} 