using Backend.Identity.DependencyInjection;
using Backend.Persistence.DependencyInjection;
using Backend.Application.DependencyInjection;
using Backend.Infrastructure.DependencyInjection;
using Backend.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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

// Register Persistence services
builder.Services.ConfigurePersistenceServices(builder.Configuration);

// Register Application services
builder.Services.ConfigureApplicationServices();

// Register Infrastructure services
builder.Services.ConfigureInfrastructureServices(builder.Configuration);

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

app.Run();
