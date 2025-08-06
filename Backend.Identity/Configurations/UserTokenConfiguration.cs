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
    public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            // Table name
            builder.ToTable("AspNetUserTokens");

            // Primary key (composite key for UserToken)
            builder.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });

            // Properties
            builder.Property(ut => ut.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ut => ut.LoginProvider)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(ut => ut.Name)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(ut => ut.Value)
                .HasMaxLength(4000);

            builder.Property(ut => ut.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(ut => ut.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ut => ut.UpdatedBy)
                .HasMaxLength(450);

            builder.Property(ut => ut.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(ut => ut.DeviceInfo)
                .HasMaxLength(1000);

            builder.Property(ut => ut.IpAddress)
                .HasMaxLength(45);

            builder.Property(ut => ut.UserAgent)
                .HasMaxLength(1000);

            builder.Property(ut => ut.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ut => ut.RevokedBy)
                .HasMaxLength(450);

            builder.Property(ut => ut.RevocationReason)
                .HasMaxLength(500);

            builder.Property(ut => ut.ExpiresAt);

            builder.Property(ut => ut.RevokedAt);

            // Indexes
            builder.HasIndex(ut => ut.UserId)
                .HasDatabaseName("IX_AspNetUserTokens_UserId");

            builder.HasIndex(ut => ut.IsActive)
                .HasDatabaseName("IX_AspNetUserTokens_IsActive");

            builder.HasIndex(ut => ut.IsRevoked)
                .HasDatabaseName("IX_AspNetUserTokens_IsRevoked");

            builder.HasIndex(ut => ut.CreatedAt)
                .HasDatabaseName("IX_AspNetUserTokens_CreatedAt");

            builder.HasIndex(ut => ut.ExpiresAt)
                .HasDatabaseName("IX_AspNetUserTokens_ExpiresAt");

            builder.HasIndex(ut => ut.RevokedAt)
                .HasDatabaseName("IX_AspNetUserTokens_RevokedAt");

            builder.HasIndex(ut => ut.IpAddress)
                .HasDatabaseName("IX_AspNetUserTokens_IpAddress");

            builder.HasIndex(ut => ut.DeviceInfo)
                .HasDatabaseName("IX_AspNetUserTokens_DeviceInfo");

            builder.HasIndex(ut => ut.CreatedBy)
                .HasDatabaseName("IX_AspNetUserTokens_CreatedBy");

            builder.HasIndex(ut => ut.RevokedBy)
                .HasDatabaseName("IX_AspNetUserTokens_RevokedBy");

            // Composite indexes
            builder.HasIndex(ut => new { ut.UserId, ut.IsActive })
                .HasDatabaseName("IX_AspNetUserTokens_UserId_IsActive");

            builder.HasIndex(ut => new { ut.UserId, ut.IsRevoked })
                .HasDatabaseName("IX_AspNetUserTokens_UserId_IsRevoked");

            builder.HasIndex(ut => new { ut.LoginProvider, ut.Name })
                .HasDatabaseName("IX_AspNetUserTokens_LoginProvider_Name");
        }
    }
}
