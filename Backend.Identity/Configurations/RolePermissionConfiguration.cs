using Backend.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Identity.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            // Table name
            builder.ToTable("RolePermissions");

            // Primary key
            builder.HasKey(rp => rp.Id);

            // Properties Configuration
            builder.Property(rp => rp.Id)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(rp => rp.RoleId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(rp => rp.PermissionId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(rp => rp.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(rp => rp.UpdatedAt);

            builder.Property(rp => rp.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(rp => rp.UpdatedBy)
                .HasMaxLength(450);

            builder.Property(rp => rp.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(rp => rp.AssignedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(rp => rp.ExpiresAt);

            builder.Property(rp => rp.AssignedBy)
                .HasMaxLength(450);

            builder.Property(rp => rp.AssignmentReason)
                .HasMaxLength(500);

            builder.Property(rp => rp.IsGranted)
                .IsRequired()
                .HasDefaultValue(true);

            // Foreign Keys
            builder.HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique()
                .HasDatabaseName("IX_RolePermissions_RoleId_PermissionId");

            builder.HasIndex(rp => rp.RoleId)
                .HasDatabaseName("IX_RolePermissions_RoleId");

            builder.HasIndex(rp => rp.PermissionId)
                .HasDatabaseName("IX_RolePermissions_PermissionId");

            builder.HasIndex(rp => rp.IsActive)
                .HasDatabaseName("IX_RolePermissions_IsActive");

            builder.HasIndex(rp => rp.IsGranted)
                .HasDatabaseName("IX_RolePermissions_IsGranted");

            builder.HasIndex(rp => rp.ExpiresAt)
                .HasDatabaseName("IX_RolePermissions_ExpiresAt");

            builder.HasIndex(rp => rp.AssignedAt)
                .HasDatabaseName("IX_RolePermissions_AssignedAt");
        }
    }
}