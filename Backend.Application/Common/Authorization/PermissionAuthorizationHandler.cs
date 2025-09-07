using Microsoft.AspNetCore.Authorization;
using Backend.Application.Common.Interfaces.Identity;

namespace Backend.Application.Common.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly Backend.Application.Common.Interfaces.Identity.IAuthorizationService _authorizationService;
        private readonly ICurrentUserService _currentUserService;

        public PermissionAuthorizationHandler(Backend.Application.Common.Interfaces.Identity.IAuthorizationService authorizationService, ICurrentUserService currentUserService)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
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

            // Check if user has the required permission
            var hasPermission = await _authorizationService.HasPermissionAsync(userId, requirement.Resource, requirement.Action);
            
            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}