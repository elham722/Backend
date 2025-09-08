using Client.MVC.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Client.MVC.Middleware;

/// <summary>
/// Middleware to control access to Admin Area
/// Redirects unauthorized users to Access Denied or Login page
/// </summary>
public class AdminAreaAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AdminAreaAccessMiddleware> _logger;

    public AdminAreaAccessMiddleware(RequestDelegate next, ILogger<AdminAreaAccessMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        // Check if the request is for Admin Area
        if (IsAdminAreaRequest(context.Request.Path))
        {
            _logger.LogDebug("Admin area request detected: {Path}", context.Request.Path);

            // Check if user is authenticated
            if (!currentUser.IsAuthenticated())
            {
                _logger.LogWarning("Unauthenticated user attempted to access admin area: {Path}", context.Request.Path);
                
                // Redirect to main site login with return URL
                var returnUrl = context.Request.Path + context.Request.QueryString;
                var loginUrl = $"/Auth/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
                
                context.Response.Redirect(loginUrl);
                return;
            }

            // Check if user has Admin or SuperAdmin role
            var userRoles = currentUser.GetUserRoles();
            if (userRoles == null || (!userRoles.Contains("Admin") && !userRoles.Contains("SuperAdmin")))
            {
                _logger.LogWarning("Non-admin user attempted to access admin area: {UserId}, {Path}", 
                    currentUser.GetUserId(), context.Request.Path);
                
                // Redirect to Access Denied page for logged-in non-admin users
                context.Response.Redirect("/Home/AccessDenied");
                return;
            }

            _logger.LogDebug("Admin user authorized for admin area: {UserId}", currentUser.GetUserId());
        }

        // Continue to the next middleware
        await _next(context);
    }

    /// <summary>
    /// Checks if the request path is for Admin Area
    /// </summary>
    /// <param name="path">Request path</param>
    /// <returns>True if it's an admin area request</returns>
    private static bool IsAdminAreaRequest(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
        
        // Check for /Admin/ pattern (case insensitive)
        return pathValue.StartsWith("/admin/", StringComparison.OrdinalIgnoreCase) ||
               pathValue.Equals("/admin", StringComparison.OrdinalIgnoreCase);
    }
}