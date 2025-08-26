using System;

namespace Backend.Domain.Events.Identity
{
    public class RoleCreatedEvent : BaseDomainEvent
    {
        public Guid RoleId { get; }
        public string RoleName { get; }

        public RoleCreatedEvent(Guid roleId, string roleName)
        {
            RoleId = roleId;
            RoleName = roleName;
        }
    }
} 