using Microsoft.AspNetCore.Authorization;
using Backend.Application.Common.Interfaces.Identity;

namespace Backend.Application.Common.Authorization
{
    public class RoleRequirement : IAuthorizationRequirement
    {
        public string RoleName { get; }

        public RoleRequirement(string roleName)
        {
            RoleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
        }
    }

    public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly Backend.Application.Common.Interfaces.Identity.IAuthorizationService _authorizationService;
        private readonly ICurrentUserService _currentUserService;

        public RoleAuthorizationHandler(Backend.Application.Common.Interfaces.Identity.IAuthorizationService authorizationService, ICurrentUserService currentUserService)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            if (!_currentUserService.IsAuthenticated(context.User))
            {
                return;
            }

            var userId = _currentUserService.GetCurrentUserId(context.User);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            // Check if user has the required role
            var hasRole = await _authorizationService.IsInRoleAsync(userId, requirement.RoleName);
            
            if (hasRole)
            {
                context.Succeed(requirement);
            }
        }
    }
}