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
    public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
    {
        public void Configure(EntityTypeBuilder<UserLogin> builder)
        {
            // Table name
            builder.ToTable("AspNetUserLogins");

            // Primary key (composite key for UserLogin)
            builder.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });

            // Properties
            builder.Property(ul => ul.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ul => ul.LoginProvider)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(ul => ul.ProviderKey)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(ul => ul.ProviderDisplayName)
                .HasMaxLength(256);

            builder.Property(ul => ul.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(ul => ul.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ul => ul.UpdatedBy)
                .HasMaxLength(450);

            builder.Property(ul => ul.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(ul => ul.DeviceInfo)
                .HasMaxLength(1000);

            builder.Property(ul => ul.IpAddress)
                .HasMaxLength(45); // IPv6 max length

            builder.Property(ul => ul.UserAgent)
                .HasMaxLength(1000);

            // Indexes
            builder.HasIndex(ul => ul.UserId)
                .HasDatabaseName("IX_AspNetUserLogins_UserId");

            builder.HasIndex(ul => ul.IsActive)
                .HasDatabaseName("IX_AspNetUserLogins_IsActive");

            builder.HasIndex(ul => ul.CreatedAt)
                .HasDatabaseName("IX_AspNetUserLogins_CreatedAt");

            builder.HasIndex(ul => ul.IpAddress)
                .HasDatabaseName("IX_AspNetUserLogins_IpAddress");

            builder.HasIndex(ul => ul.DeviceInfo)
                .HasDatabaseName("IX_AspNetUserLogins_DeviceInfo");

            builder.HasIndex(ul => ul.CreatedBy)
                .HasDatabaseName("IX_AspNetUserLogins_CreatedBy");

            // Composite indexes
            builder.HasIndex(ul => new { ul.UserId, ul.IsActive })
                .HasDatabaseName("IX_AspNetUserLogins_UserId_IsActive");

            builder.HasIndex(ul => new { ul.LoginProvider, ul.ProviderKey })
                .HasDatabaseName("IX_AspNetUserLogins_LoginProvider_ProviderKey");
        }
    }
}
