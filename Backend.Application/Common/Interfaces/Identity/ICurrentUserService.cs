namespace Backend.Application.Common.Interfaces.Identity
{
    /// <summary>
    /// Service interface for getting current user information from claims
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the current user ID from claims
        /// </summary>
        string? GetCurrentUserId(System.Security.Claims.ClaimsPrincipal user);
        
        /// <summary>
        /// Gets the current user ID from claims, throws if not found
        /// </summary>
        string GetCurrentUserIdOrThrow(System.Security.Claims.ClaimsPrincipal user);
        
        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        bool IsAuthenticated(System.Security.Claims.ClaimsPrincipal user);
        
        /// <summary>
        /// Gets the current user's email from claims
        /// </summary>
        string? GetCurrentUserEmail(System.Security.Claims.ClaimsPrincipal user);
        
        /// <summary>
        /// Gets the current user's username from claims
        /// </summary>
        string? GetCurrentUserName(System.Security.Claims.ClaimsPrincipal user);
    }
}