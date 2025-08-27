using Backend.Domain.Entities;
using Backend.Domain.ValueObjects;
using Backend.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Customer entity
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // Table name
        builder.ToTable("Customers");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.MiddleName)
            .HasMaxLength(100);

        builder.Property(c => c.DateOfBirth);

        builder.Property(c => c.Gender)
            .HasConversion<string>();

        builder.Property(c => c.PassportNumber)
            .HasMaxLength(50);

        builder.Property(c => c.CustomerStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.CompanyName)
            .HasMaxLength(200);

        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        builder.Property(c => c.ApplicationUserId)
            .HasMaxLength(450);

        // Value Objects - Email (nullable)
        builder.Property(c => c.Email)
            .HasConversion(new EmailConverter())
            .HasMaxLength(255);

        // Value Objects - PhoneNumber (nullable)
        builder.Property(c => c.PhoneNumber)
            .HasConversion(new PhoneNumberConverter())
            .HasMaxLength(20);

        // Value Objects - MobileNumber (nullable)
        builder.Property(c => c.MobileNumber)
            .HasConversion(new PhoneNumberConverter())
            .HasMaxLength(20);

        // Value Objects - NationalCode (nullable)
        builder.Property(c => c.NationalCode)
            .HasConversion(new NationalCodeConverter())
            .HasMaxLength(10);

        // Value Objects - PrimaryAddress (using OwnsOne for complex value objects)
        builder.OwnsOne(c => c.PrimaryAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Street")
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("City")
                .HasMaxLength(100);

            address.Property(a => a.Country)
                .HasColumnName("Country")
                .HasMaxLength(100);

            address.Property(a => a.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(20);

            address.Property(a => a.Province)
                .HasColumnName("Province")
                .HasMaxLength(100);
        });

        // Audit fields
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.HasIndex(c => c.PhoneNumber)
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL");

        builder.HasIndex(c => c.MobileNumber)
            .IsUnique()
            .HasFilter("[MobileNumber] IS NOT NULL");

        builder.HasIndex(c => c.NationalCode)
            .IsUnique()
            .HasFilter("[NationalCode] IS NOT NULL");

        builder.HasIndex(c => c.ApplicationUserId)
            .IsUnique()
            .HasFilter("[ApplicationUserId] IS NOT NULL");

        builder.HasIndex(c => c.FirstName);
        builder.HasIndex(c => c.LastName);
        builder.HasIndex(c => c.CustomerStatus);
        builder.HasIndex(c => c.CreatedAt);
        builder.HasIndex(c => c.DateOfBirth);

        // Composite indexes
        builder.HasIndex(c => new { c.FirstName, c.LastName });
    }
} 