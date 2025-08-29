using Client.MVC.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure forwarded headers for proxy scenarios
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add distributed memory cache for session support
builder.Services.AddDistributedMemoryCache();

// Add session support with improved security
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(
        builder.Configuration.GetValue<int>("CookieSecurity:ExpirationMinutes", 30));
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
    options.DefaultSignInScheme = "JwtBearer";
})
.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, JwtAuthenticationHandler>("JwtBearer", options => { });

// Configure Typed HttpClient with settings from configuration
var apiSettings = builder.Configuration.GetSection("ApiSettings");
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiSettings["BaseUrl"] ?? "https://localhost:7209/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(
        apiSettings.GetValue<int>("TimeoutSeconds", 30));
});

// Register services with improved dependency injection
builder.Services.AddScoped<IAuthenticationInterceptor, AuthenticationInterceptor>();
builder.Services.AddSingleton<ResiliencePolicyService>();
builder.Services.AddScoped<IAuthenticatedHttpClient, AuthenticatedHttpClient>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<IUserApiClient, UserApiClient>();
builder.Services.AddScoped<IBackgroundJobAuthClient, BackgroundJobAuthClient>();

// Register new services
builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddSingleton<LogSanitizer>();
builder.Services.AddSingleton<IConcurrencyManager, ConcurrencyManager>();
builder.Services.AddScoped<ITokenManager, TokenManager>();

builder.Services.AddHttpContextAccessor();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    // Enable HSTS if configured
    if (builder.Configuration.GetValue<bool>("Security:EnableHsts", true))
    {
        app.UseHsts();
    }
}

// Use forwarded headers
app.UseForwardedHeaders();

// Global exception handling is handled by the built-in error handling

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add security headers
if (builder.Configuration.GetValue<bool>("Security:EnableCsp", true))
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline';");
        await next();
    });
}

if (builder.Configuration.GetValue<bool>("Security:EnableXssProtection", true))
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        await next();
    });
}

if (builder.Configuration.GetValue<bool>("Security:EnableFrameOptions", true))
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        await next();
    });
}

app.UseRouting();

// Use session
app.UseSession();

// Use authentication (must come before authorization)
app.UseAuthentication();
app.UseAuthorization();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
