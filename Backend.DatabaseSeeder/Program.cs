using Backend.Identity.Services;
using Backend.Identity.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Backend.DatabaseSeeder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/seeder-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting Database Seeder...");

                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                    .Build();

                // Build host
                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices((context, services) =>
                    {
                        // Register Identity services
                        services.ConfigureIdentityServices(configuration);
                    })
                    .UseSerilog()
                    .Build();

                // Run seeding
                using var scope = host.Services.CreateScope();
                await Backend.Identity.Services.DatabaseSeeder.SeedDatabaseAsync(scope.ServiceProvider, configuration);

                Log.Information("Database seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database seeding failed!");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}