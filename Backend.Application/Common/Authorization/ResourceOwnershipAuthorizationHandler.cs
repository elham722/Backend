using Microsoft.AspNetCore.Authorization;
using Backend.Application.Common.Interfaces.Identity;

namespace Backend.Application.Common.Authorization
{
    public class ResourceOwnershipRequirement : IAuthorizationRequirement
    {
        public string ResourceType { get; }
        public string ResourceIdClaimType { get; }

        public ResourceOwnershipRequirement(string resourceType, string resourceIdClaimType = "ResourceId")
        {
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            ResourceIdClaimType = resourceIdClaimType ?? throw new ArgumentNullException(nameof(resourceIdClaimType));
        }
    }

    public class ResourceOwnershipAuthorizationHandler : AuthorizationHandler<ResourceOwnershipRequirement>
    {
        private readonly Backend.Application.Common.Interfaces.Identity.IAuthorizationService _authorizationService;
        private readonly ICurrentUserService _currentUserService;

        public ResourceOwnershipAuthorizationHandler(Backend.Application.Common.Interfaces.Identity.IAuthorizationService authorizationService, ICurrentUserService currentUserService)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceOwnershipRequirement requirement)
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

            // Get resource ID from claim or route
            var resourceId = context.User.FindFirst(requirement.ResourceIdClaimType)?.Value;
            if (string.IsNullOrEmpty(resourceId))
            {
                // Try to get from route values if not in claims
                if (context.Resource is Microsoft.AspNetCore.Http.HttpContext httpContext)
                {
                    resourceId = httpContext.Request.RouteValues[requirement.ResourceIdClaimType]?.ToString();
                }
            }

            if (string.IsNullOrEmpty(resourceId))
            {
                return;
            }

            // Check if user owns the resource
            var isOwner = await _authorizationService.IsResourceOwnerAsync(userId, requirement.ResourceType, resourceId);
            
            if (isOwner)
            {
                context.Succeed(requirement);
            }
        }
    }
}