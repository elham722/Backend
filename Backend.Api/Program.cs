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

// Register Infrastructure services
builder.Services.ConfigureInfrastructureServices(builder.Configuration);

// Register MediatR for API layer
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

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

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
