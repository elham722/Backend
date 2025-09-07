using Backend.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Identity.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // Table name
            builder.ToTable("AspNetRoles");

            // Primary key
            builder.HasKey(r => r.Id);

            // Properties Configuration
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(r => r.NormalizedName)
                .HasMaxLength(256);

            builder.Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(r => r.UpdatedAt);

            builder.Property(r => r.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.UpdatedBy)
                .HasMaxLength(450);

            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(r => r.IsSystemRole)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(r => r.Priority)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(r => r.Category)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(r => r.Name)
                .IsUnique()
                .HasDatabaseName("IX_AspNetRoles_Name");

            builder.HasIndex(r => r.NormalizedName)
                .IsUnique()
                .HasDatabaseName("IX_AspNetRoles_NormalizedName");

            builder.HasIndex(r => r.IsActive)
                .HasDatabaseName("IX_AspNetRoles_IsActive");

            builder.HasIndex(r => r.IsSystemRole)
                .HasDatabaseName("IX_AspNetRoles_IsSystemRole");

            builder.HasIndex(r => r.Category)
                .HasDatabaseName("IX_AspNetRoles_Category");

            builder.HasIndex(r => r.Priority)
                .HasDatabaseName("IX_AspNetRoles_Priority");

            builder.HasIndex(r => r.CreatedAt)
                .HasDatabaseName("IX_AspNetRoles_CreatedAt");
        }
    }
}