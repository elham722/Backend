using Backend.Infrastructure.ExternalServices;
using Backend.Infrastructure.LocalStorage;

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

// Add HttpClient for ExternalService
builder.Services.AddHttpClient();

// Register only the necessary services for MVC client
builder.Services.AddScoped<Backend.Infrastructure.ExternalServices.IExternalService, Backend.Infrastructure.ExternalServices.ExternalServiceSimple>();
builder.Services.AddScoped<Backend.Infrastructure.LocalStorage.ILocalStorageService, Backend.Infrastructure.LocalStorage.LocalStorageService>();

// Register token service

builder.Services.AddHttpContextAccessor();

// Configure ExternalService options
builder.Services.Configure<Backend.Infrastructure.ExternalServices.ExternalServiceOptions>(options =>
{
    options.BaseUrl = "https://localhost:7209/"; // Backend API URL
    options.TimeoutSeconds = 30;
    options.UseHttps = true;
    options.DefaultHeaders = new Dictionary<string, string>
    {
        { "Accept", "application/json" }
    };
});

// Configure LocalStorage options
builder.Services.Configure<Backend.Infrastructure.LocalStorage.LocalStorageOptions>(options =>
{
    options.Directory = "LocalStorage";
    options.Filename = "ClientMVC.LocalStorage";
    options.AutoLoad = true;
    options.AutoSave = true;
});

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
