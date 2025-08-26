using System;

namespace Backend.Domain.Events.Identity
{
    public class RoleActivatedEvent : BaseDomainEvent
    {
        public Guid RoleId { get; }
        public string RoleName { get; }

        public RoleActivatedEvent(Guid roleId, string roleName)
        {
            RoleId = roleId;
            RoleName = roleName;
        }
    }
} 