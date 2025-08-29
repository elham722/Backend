using Client.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add distributed cache for session support
builder.Services.AddDistributedMemoryCache();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Typed HttpClient for API communication
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7209/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register authentication interceptor
builder.Services.AddScoped<Client.MVC.Services.IAuthenticationInterceptor, Client.MVC.Services.AuthenticationInterceptor>();

// Register authenticated HTTP client
builder.Services.AddScoped<Client.MVC.Services.IAuthenticatedHttpClient, Client.MVC.Services.AuthenticatedHttpClient>();

// Register session management service
builder.Services.AddScoped<Client.MVC.Services.IUserSessionService, Client.MVC.Services.UserSessionService>();

// Register AuthApiClient
builder.Services.AddScoped<Client.MVC.Services.IAuthApiClient, Client.MVC.Services.AuthApiClient>();

// Register UserApiClient
builder.Services.AddScoped<Client.MVC.Services.IUserApiClient, Client.MVC.Services.UserApiClient>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Use session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
