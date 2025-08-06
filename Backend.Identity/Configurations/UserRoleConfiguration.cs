using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Identity.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            // Table name
            builder.ToTable("AspNetUserRoles");

            // Primary key (composite key for UserRole)
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            // Properties
            builder.Property(ur => ur.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ur => ur.RoleId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ur => ur.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(ur => ur.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ur => ur.UpdatedBy)
                .HasMaxLength(450);

            builder.Property(ur => ur.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(ur => ur.AssignedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(ur => ur.AssignedBy)
                .HasMaxLength(450);

            builder.Property(ur => ur.AssignmentReason)
                .HasMaxLength(500);

            builder.Property(ur => ur.ExpiresAt);

            // Indexes
            builder.HasIndex(ur => ur.UserId)
                .HasDatabaseName("IX_AspNetUserRoles_UserId");

            builder.HasIndex(ur => ur.RoleId)
                .HasDatabaseName("IX_AspNetUserRoles_RoleId");

            builder.HasIndex(ur => ur.IsActive)
                .HasDatabaseName("IX_AspNetUserRoles_IsActive");

            builder.HasIndex(ur => ur.CreatedAt)
                .HasDatabaseName("IX_AspNetUserRoles_CreatedAt");

            builder.HasIndex(ur => ur.ExpiresAt)
                .HasDatabaseName("IX_AspNetUserRoles_ExpiresAt");

            builder.HasIndex(ur => ur.AssignedAt)
                .HasDatabaseName("IX_AspNetUserRoles_AssignedAt");

            builder.HasIndex(ur => ur.AssignedBy)
                .HasDatabaseName("IX_AspNetUserRoles_AssignedBy");

            // Composite indexes
            builder.HasIndex(ur => new { ur.UserId, ur.IsActive })
                .HasDatabaseName("IX_AspNetUserRoles_UserId_IsActive");

            builder.HasIndex(ur => new { ur.RoleId, ur.IsActive })
                .HasDatabaseName("IX_AspNetUserRoles_RoleId_IsActive");

            builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
                .HasDatabaseName("IX_AspNetUserRoles_UserId_RoleId");
        }
    }
}
