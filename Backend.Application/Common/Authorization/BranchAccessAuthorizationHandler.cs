using Microsoft.AspNetCore.Authorization;
using Backend.Application.Common.Interfaces.Identity;

namespace Backend.Application.Common.Authorization
{
    public class BranchAccessRequirement : IAuthorizationRequirement
    {
        public string BranchIdClaimType { get; }

        public BranchAccessRequirement(string branchIdClaimType = "BranchId")
        {
            BranchIdClaimType = branchIdClaimType ?? throw new ArgumentNullException(nameof(branchIdClaimType));
        }
    }

    public class BranchAccessAuthorizationHandler : AuthorizationHandler<BranchAccessRequirement>
    {
        private readonly Backend.Application.Common.Interfaces.Identity.IAuthorizationService _authorizationService;
        private readonly ICurrentUserService _currentUserService;

        public BranchAccessAuthorizationHandler(Backend.Application.Common.Interfaces.Identity.IAuthorizationService authorizationService, ICurrentUserService currentUserService)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BranchAccessRequirement requirement)
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

            // Get branch ID from claim
            var userBranchId = context.User.FindFirst(requirement.BranchIdClaimType)?.Value;
            if (string.IsNullOrEmpty(userBranchId))
            {
                return;
            }

            // Get target branch ID from route or query
            string? targetBranchId = null;
            if (context.Resource is Microsoft.AspNetCore.Http.HttpContext httpContext)
            {
                targetBranchId = httpContext.Request.RouteValues["branchId"]?.ToString() ??
                                httpContext.Request.Query["branchId"].FirstOrDefault();
            }

            if (string.IsNullOrEmpty(targetBranchId))
            {
                return;
            }

            // Check if user has access to the target branch
            var hasAccess = await _authorizationService.HasBranchAccessAsync(userId, targetBranchId);
            
            if (hasAccess)
            {
                context.Succeed(requirement);
            }
        }
    }
}