using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Extensions
{
    /// <summary>
    /// CSRF Protection Middleware
    /// </summary>
    public class CsrfProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CsrfProtectionMiddleware> _logger;

        public CsrfProtectionMiddleware(RequestDelegate next, ILogger<CsrfProtectionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip CSRF check for GET requests and OPTIONS requests
            if (context.Request.Method == "GET" || context.Request.Method == "OPTIONS")
            {
                await _next(context);
                return;
            }

            // Check if request has valid CSRF token
            if (!IsValidCsrfToken(context))
            {
                _logger.LogWarning("CSRF token validation failed for request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("CSRF token validation failed");
                return;
            }

            await _next(context);
        }

        private bool IsValidCsrfToken(HttpContext context)
        {
            // Get CSRF token from header
            var csrfToken = context.Request.Headers["X-CSRF-TOKEN"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(csrfToken))
            {
                return false;
            }

            // Get session token
            var sessionToken = context.Session.GetString("CSRF_TOKEN");
            
            if (string.IsNullOrEmpty(sessionToken))
            {
                return false;
            }

            // Compare tokens
            return csrfToken.Equals(sessionToken, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Extension method for adding CSRF protection
    /// </summary>
    public static class CsrfProtectionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsrfProtection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CsrfProtectionMiddleware>();
        }
    }
} 