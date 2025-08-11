using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Identity.Configurations;
using Backend.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Backend.Identity.Context
{
    public class BackendIdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public BackendIdentityDbContext(DbContextOptions<BackendIdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply configurations
            builder.ApplyConfiguration(new ApplicationUserConfiguration());

            // Add composite indexes for better performance
            AddCompositeIndexes(builder);
        }

        private void AddCompositeIndexes(ModelBuilder builder)
        {
            // Note: Since we're using default Identity entities, we can't add custom indexes
            // to UserClaim, UserLogin, UserRole, and UserToken tables
            // Only ApplicationUser custom properties can be indexed
        }
    }

    // Design-time factory for migrations
    public class BackendIdentityDbContextFactory : IDesignTimeDbContextFactory<BackendIdentityDbContext>
    {
        public BackendIdentityDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BackendIdentityDbContext>();
            
            // Find the solution directory by looking for .sln file
            var solutionDir = FindSolutionDirectory();
            var configPath = Path.Combine(solutionDir, "Backend.Api", "appsettings.json");
            
            if (!File.Exists(configPath))
            {
                throw new InvalidOperationException($"Configuration file not found at: {configPath}");
            }
            
            // Get the configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(solutionDir)
                .AddJsonFile("Backend.Api/appsettings.json", optional: false)
                .AddJsonFile("Backend.Api/appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("IdentityDBConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'IdentityDBConnection' not found in configuration.");
            }

            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(BackendIdentityDbContext).Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            return new BackendIdentityDbContext(optionsBuilder.Options);
        }
        
        private static string FindSolutionDirectory()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var solutionFile = Directory.GetFiles(currentDir, "*.sln").FirstOrDefault();
            
            if (solutionFile != null)
            {
                return currentDir;
            }
            
            // Look in parent directories
            var parentDir = Directory.GetParent(currentDir);
            while (parentDir != null)
            {
                solutionFile = Directory.GetFiles(parentDir.FullName, "*.sln").FirstOrDefault();
                if (solutionFile != null)
                {
                    return parentDir.FullName;
                }
                parentDir = parentDir.Parent;
            }
            
            throw new InvalidOperationException("Solution directory not found");
        }
    }
}
