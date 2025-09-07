using Backend.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Identity.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            // Table name
            builder.ToTable("Permissions");

            // Primary key
            builder.HasKey(p => p.Id);

            // Properties Configuration
            builder.Property(p => p.Id)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Resource)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.UpdatedAt);

            builder.Property(p => p.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(p => p.UpdatedBy)
                .HasMaxLength(450);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.IsSystemPermission)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.Category)
                .HasMaxLength(100);

            builder.Property(p => p.Priority)
                .IsRequired()
                .HasDefaultValue(0);

            // Indexes
            builder.HasIndex(p => p.Name)
                .IsUnique()
                .HasDatabaseName("IX_Permissions_Name");

            builder.HasIndex(p => new { p.Resource, p.Action })
                .IsUnique()
                .HasDatabaseName("IX_Permissions_Resource_Action");

            builder.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_Permissions_IsActive");

            builder.HasIndex(p => p.IsSystemPermission)
                .HasDatabaseName("IX_Permissions_IsSystemPermission");

            builder.HasIndex(p => p.Category)
                .HasDatabaseName("IX_Permissions_Category");

            builder.HasIndex(p => p.Priority)
                .HasDatabaseName("IX_Permissions_Priority");

            builder.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Permissions_CreatedAt");
        }
    }
}