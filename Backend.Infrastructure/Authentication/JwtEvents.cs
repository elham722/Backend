using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Backend.Infrastructure.Authentication
{
    /// <summary>
    /// JWT Bearer authentication events handler
    /// </summary>
    public static class JwtEvents
    {
        public static Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }

        public static Task OnTokenValidated(TokenValidatedContext context)
        {
            // Additional token validation can be added here
            // For example: check if user is still active, check permissions, etc.
            
            // Example: Add custom claims if needed
            var identity = context.Principal?.Identity as ClaimsIdentity;
            if (identity != null)
            {
                // Add any additional claims here
                // identity.AddClaim(new Claim("custom_claim", "value"));
            }
            
            return Task.CompletedTask;
        }

        public static Task OnMessageReceived(MessageReceivedContext context)
        {
            // Extract token from query string if needed (for SignalR)
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }

        public static Task OnChallenge(JwtBearerChallengeContext context)
        {
            // Customize challenge response if needed
            return Task.CompletedTask;
        }

        public static Task OnForbidden(ForbiddenContext context)
        {
            // Handle forbidden access
            return Task.CompletedTask;
        }
    }
}