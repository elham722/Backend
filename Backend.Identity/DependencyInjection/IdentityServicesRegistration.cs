using System.Reflection;
using Backend.Identity.Context;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Backend.Application.Common.Interfaces;
using Backend.Identity.Services;
using Microsoft.AspNetCore.Http;

namespace Backend.Identity.DependencyInjection
{
    public static class IdentityServicesRegistration
    {
        public static IServiceCollection ConfigureIdentityServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure DbContext
            ConfigureDbContext(services, configuration);

            // Configure Identity
            ConfigureIdentity(services);

            // Configure AutoMapper
            services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

            // Add Identity services
            services.AddIdentityServices();

            // Add mapping services
            services.AddMappingServices();

            return services;
        }

        private static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration)
        {
            // Get connection string
            var connectionString = configuration.GetConnectionString("IdentityDBConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'IdentityDBConnection' not found in configuration.");
            }

            // Add DbContext
            services.AddDbContext<BackendIdentityDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(BackendIdentityDbContext).Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

              
            });

        }

        private static void ConfigureIdentity(IServiceCollection services)
        {
            // Configure Identity with custom entities
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings - Enhanced security
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 12; // Increased from 8
                options.Password.RequiredUniqueChars = 3; // Increased from 1

                // Lockout settings - Enhanced security
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); // Increased from 15
                options.Lockout.MaxFailedAccessAttempts = 3; // Reduced from 5
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // SignIn settings - Enhanced security
                options.SignIn.RequireConfirmedAccount = true; // ✅ Changed to true
                options.SignIn.RequireConfirmedEmail = true; // ✅ Changed to true
                options.SignIn.RequireConfirmedPhoneNumber = false; // Keep false for now

                // Token settings
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.AuthenticatorIssuer = "Backend.Identity"; // ✅ Added
            })
            .AddEntityFrameworkStores<BackendIdentityDbContext>()
            .AddDefaultTokenProviders();

            // Configure Identity options from configuration
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings - Enhanced security
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 12; // Increased from 8
                options.Password.RequiredUniqueChars = 3; // Increased from 1

                // Lockout settings - Enhanced security
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); // Increased from 15
                options.Lockout.MaxFailedAccessAttempts = 3; // Reduced from 5
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // SignIn settings - Enhanced security
                options.SignIn.RequireConfirmedAccount = true; // ✅ Changed to true
                options.SignIn.RequireConfirmedEmail = true; // ✅ Changed to true
                options.SignIn.RequireConfirmedPhoneNumber = false; // Keep false for now
            });

            // Configure cookie settings
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ✅ Enhanced
                options.Cookie.SameSite = SameSiteMode.Strict; // ✅ Enhanced
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // ✅ Reduced from 60
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context => // ✅ Enhanced
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });

            // Configure external cookie settings
            services.ConfigureExternalCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ✅ Enhanced
                options.Cookie.SameSite = SameSiteMode.Strict; // ✅ Enhanced
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // ✅ Reduced from 60
                options.SlidingExpiration = true;
            });

            

            // Add HttpClient for social auth
          //  services.AddHttpClient();

            // Add Memory Cache for refresh tokens
            services.AddMemoryCache();

            // Add MediatR for Identity layer
            //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IdentityServicesRegistration).Assembly));
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            // Register Identity services
            services.AddScoped<IAccountManagementService, AccountManagementService>();
            services.AddScoped<ITwoFactorService, TwoFactorService>();
            services.AddScoped<ISocialLoginService, SocialLoginService>();
            services.AddScoped<IEmailConfirmationService, EmailConfirmationService>(); // ✅ Added
            
            // Register UserService
            services.AddScoped<IUserService, UserService>();
            
            // Register RefreshTokenService
            services.AddScoped<Backend.Domain.Interfaces.IRefreshTokenService, RefreshTokenService>();
            
            // Register default date time service if not already registered
            services.AddScoped<IDateTimeService, DefaultDateTimeService>();

            return services;
        }
        /// <summary>
        /// Registers mapping services
        /// </summary>
        private static IServiceCollection AddMappingServices(this IServiceCollection services)
        {
            // AutoMapper is already registered in Application layer
            // Identity adapters are registered in Infrastructure layer
            // No additional mapping services needed here

            return services;
        }
    }
}
