using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Domain.ValueObjects;
using Backend.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Persistence.Seeders;

/// <summary>
/// Database seeder for initial data
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="logger">Logger</param>
    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Check if data already exists
            if (await context.Customers.AnyAsync())
            {
                logger.LogInformation("Database already contains data. Skipping seeding.");
                return;
            }

            logger.LogInformation("Starting database seeding...");

            // Seed customers
            await SeedCustomersAsync(context, logger);

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    /// <summary>
    /// Seeds customer data
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger</param>
    private static async Task SeedCustomersAsync(ApplicationDbContext context, ILogger logger)
    {
        var customers = new List<Customer>();

        // Premium Customer
        var premiumCustomer = Customer.Create(
            firstName: "John",
            lastName: "Doe",
            email: Email.Create("john.doe@example.com"),
            phoneNumber: PhoneNumber.Create("+1234567890"),
            dateOfBirth: new DateTime(1990, 5, 15),
            address: Address.Create(
                country: "USA",
                province: "New York",
                city: "New York",
                district: "Manhattan",
                street: "123 Main St",
                postalCode: "10001"),
            createdBy: "System"
        );
        premiumCustomer.UpgradeToPremium("System");
        premiumCustomer.Verify("System");
        customers.Add(premiumCustomer);

        // Regular Customer
        var regularCustomer = Customer.Create(
            firstName: "Jane",
            lastName: "Smith",
            email: Email.Create("jane.smith@example.com"),
            phoneNumber: PhoneNumber.Create("+1987654321"),
            dateOfBirth: new DateTime(1985, 8, 22),
            address: Address.Create(
                country: "USA",
                province: "California",
                city: "Los Angeles",
                district: "Hollywood",
                street: "456 Oak Ave",
                postalCode: "90210"),
            createdBy: "System"
        );
        regularCustomer.Verify("System");
        customers.Add(regularCustomer);

        // Business Customer
        var businessCustomer = Customer.Create(
            firstName: "Michael",
            lastName: "Johnson",
            email: Email.Create("michael.johnson@company.com"),
            phoneNumber: PhoneNumber.Create("+1555123456"),
            dateOfBirth: new DateTime(1978, 12, 10),
            address: Address.Create(
                country: "USA",
                province: "Illinois",
                city: "Chicago",
                district: "Loop",
                street: "789 Business Blvd",
                postalCode: "60601"),
            createdBy: "System"
        );
        businessCustomer.UpgradeToPremium("System");
        businessCustomer.Verify("System");
        customers.Add(businessCustomer);

        // Young Customer
        var youngCustomer = Customer.Create(
            firstName: "Sarah",
            lastName: "Wilson",
            email: Email.Create("sarah.wilson@email.com"),
            phoneNumber: PhoneNumber.Create("+1444333222"),
            dateOfBirth: new DateTime(2000, 3, 8),
            address: Address.Create(
                country: "USA",
                province: "Florida",
                city: "Miami",
                district: "South Beach",
                street: "321 Youth St",
                postalCode: "33101"),
            createdBy: "System"
        );
        customers.Add(youngCustomer);

        // International Customer
        var internationalCustomer = Customer.Create(
            firstName: "Ahmed",
            lastName: "Hassan",
            email: Email.Create("ahmed.hassan@international.com"),
            phoneNumber: PhoneNumber.Create("+20123456789"),
            dateOfBirth: new DateTime(1992, 11, 25),
            address: Address.Create(
                country: "Egypt",
                province: "Cairo",
                city: "Cairo",
                district: "Downtown",
                street: "654 Global Rd",
                postalCode: "11511"),
            createdBy: "System"
        );
        internationalCustomer.UpgradeToPremium("System");
        customers.Add(internationalCustomer);

        // Inactive Customer
        var inactiveCustomer = Customer.Create(
            firstName: "Robert",
            lastName: "Brown",
            email: Email.Create("robert.brown@old.com"),
            phoneNumber: PhoneNumber.Create("+1777888999"),
            dateOfBirth: new DateTime(1975, 6, 18),
            address: Address.Create(
                country: "USA",
                province: "Massachusetts",
                city: "Boston",
                district: "Back Bay",
                street: "987 Old St",
                postalCode: "02101"),
            createdBy: "System"
        );
        inactiveCustomer.Deactivate("System");
        customers.Add(inactiveCustomer);

        await context.Customers.AddRangeAsync(customers);
        logger.LogInformation("Added {Count} customers to the database.", customers.Count);
    }
} 