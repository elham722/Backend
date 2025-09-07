using Microsoft.AspNetCore.Authorization;

namespace Backend.Application.Common.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Resource { get; }
        public string Action { get; }

        public PermissionRequirement(string resource, string action)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }
    }
}