using Backend.Domain.Entities;
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

        builder.Property(c => c.DateOfBirth)
            .IsRequired();

        builder.Property(c => c.CustomerStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsPremium)
            .IsRequired()
            .HasDefaultValue(false);

        // Value Objects - Email
        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255);
        });

        // Value Objects - PhoneNumber
        builder.OwnsOne(c => c.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20);
        });

        // Value Objects - PrimaryAddress
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
        builder.HasIndex(c => c.Email.Value)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.HasIndex(c => c.PhoneNumber.Value)
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL");

        builder.HasIndex(c => c.FirstName);
        builder.HasIndex(c => c.LastName);
        builder.HasIndex(c => c.CustomerStatus);
        builder.HasIndex(c => c.IsVerified);
        builder.HasIndex(c => c.IsPremium);
        builder.HasIndex(c => c.CreatedAt);
        builder.HasIndex(c => c.DateOfBirth);

        // Composite indexes
        builder.HasIndex(c => new { c.FirstName, c.LastName });
        builder.HasIndex(c => new { c.CustomerStatus, c.IsVerified });
        builder.HasIndex(c => new { c.IsPremium, c.CustomerStatus });
    }
} 