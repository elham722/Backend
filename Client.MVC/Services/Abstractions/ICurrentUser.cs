namespace Client.MVC.Services.Abstractions
{
    /// <summary>
    /// Service for getting current authenticated user information
    /// Follows Single Responsibility Principle - only handles user data retrieval
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// Get current user ID from authentication context
        /// </summary>
        /// <returns>User ID if authenticated, null otherwise</returns>
        string? GetUserId();

        /// <summary>
        /// Get current user name from authentication context
        /// </summary>
        /// <returns>User name if authenticated, null otherwise</returns>
        string? GetUserName();

        /// <summary>
        /// Get current user email from authentication context
        /// </summary>
        /// <returns>User email if authenticated, null otherwise</returns>
        string? GetUserEmail();

        /// <summary>
        /// Get current user roles from authentication context
        /// </summary>
        /// <returns>List of user roles</returns>
        IEnumerable<string> GetUserRoles();

        /// <summary>
        /// Check if user is currently authenticated
        /// </summary>
        /// <returns>True if user is authenticated, false otherwise</returns>
        bool IsAuthenticated();
    }
}