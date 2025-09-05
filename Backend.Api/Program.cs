using Asp.Versioning;
using Backend.Identity.DependencyInjection;
using Backend.Persistence.DependencyInjection;
using Backend.Application.DependencyInjection;
using Backend.Infrastructure.DependencyInjection;
using Backend.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("logs/backend-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Add CORS policy for MVC client
builder.Services.AddCors(options =>
{
    options.AddPolicy("MvcClient", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000", "https://localhost:7251", "http://localhost:5242")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Add response caching
builder.Services.AddResponseCaching();

// Add memory cache
builder.Services.AddMemoryCache();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Configure ProblemDetails
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    };
});

// Add distributed cache for session support
builder.Services.AddDistributedMemoryCache();

// Add session support for CSRF protection
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Identity services
builder.Services.ConfigureIdentityServices(builder.Configuration);



// Register Application services
builder.Services.ConfigureApplicationServices();

// Register Persistence services
builder.Services.AddPersistenceServices(builder.Configuration);

// Register Infrastructure services
builder.Services.ConfigureInfrastructureServices(builder.Configuration);

// Register MediatR for API and Application layers
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Backend.Application.Features.UserManagement.Commands.Register.RegisterCommand).Assembly);
});

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")))
    };
});

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger with versioning support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Configure Swagger to work with API versioning
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Backend API",
        Version = "v1",
        Description = "Backend API Version 1.0"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();

// Use response compression
app.UseResponseCompression();

// Use response caching
app.UseResponseCaching();

// Use CORS
app.UseCors("MvcClient");

// Add security headers
app.Use(async (context, next) =>
{
    // Security headers for API
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    
    await next();
});

// Use session
app.UseSession();

// Add global error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add rate limiting middleware
var rateLimitingOptions = new Backend.Infrastructure.Extensions.RateLimitingOptions
{
    MaxRequestsPerWindow = builder.Configuration.GetValue<int>("RateLimiting:MaxRequestsPerWindow", 100),
    WindowMinutes = builder.Configuration.GetValue<int>("RateLimiting:WindowMinutes", 1),
    EnableLogging = builder.Configuration.GetValue<bool>("RateLimiting:EnableLogging", true)
};

app.UseMiddleware<Backend.Infrastructure.Extensions.RateLimitingMiddleware>(rateLimitingOptions);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting Backend API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
