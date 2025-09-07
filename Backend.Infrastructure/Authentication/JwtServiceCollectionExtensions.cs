using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend.Application.Common.Models;

namespace Backend.Infrastructure.Authentication
{
    public static class JwtServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure strongly typed JWT settings
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() 
                ?? throw new InvalidOperationException("JWT settings not configured");

            // Validate required settings
            if (string.IsNullOrEmpty(jwtSettings.SecretKey))
                throw new InvalidOperationException("JWT SecretKey not configured");
            if (string.IsNullOrEmpty(jwtSettings.Issuer))
                throw new InvalidOperationException("JWT Issuer not configured");
            if (string.IsNullOrEmpty(jwtSettings.Audience))
                throw new InvalidOperationException("JWT Audience not configured");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidateLifetime = jwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = jwtSettings.ClockSkew,
                    RequireExpirationTime = jwtSettings.RequireExpirationTime,
                    RequireSignedTokens = jwtSettings.RequireSignedTokens
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = JwtEvents.OnAuthenticationFailed,
                    OnTokenValidated = JwtEvents.OnTokenValidated,
                    OnMessageReceived = JwtEvents.OnMessageReceived
                };
            });

            return services;
        }
    }
}