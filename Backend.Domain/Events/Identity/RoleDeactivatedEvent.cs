using System;

namespace Backend.Domain.Events.Identity
{
    public class RoleDeactivatedEvent : BaseDomainEvent
    {
        public Guid RoleId { get; }
        public string RoleName { get; }

        public RoleDeactivatedEvent(Guid roleId, string roleName)
        {
            RoleId = roleId;
            RoleName = roleName;
        }
    }
} 