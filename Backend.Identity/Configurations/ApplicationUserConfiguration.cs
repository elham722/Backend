using Backend.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Identity.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Table name
            builder.ToTable("AspNetUsers");

            // Primary key
            builder.HasKey(u => u.Id);

            // Value Objects Configuration
            ConfigureAccountInfo(builder);
            ConfigureSecurityInfo(builder);
            ConfigureAuditInfo(builder);

            // Properties Configuration
            ConfigureProperties(builder);

            // Indexes
            ConfigureIndexes(builder);

            // Navigation Properties
            ConfigureNavigationProperties(builder);
        }

        private void ConfigureAccountInfo(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.OwnsOne(u => u.Account, account =>
            {
                account.Property(a => a.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                account.Property(a => a.LastLoginAt);

                account.Property(a => a.LastPasswordChangeAt);

                account.Property(a => a.LoginAttempts)
                    .IsRequired()
                    .HasDefaultValue(0);

                account.Property(a => a.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                account.Property(a => a.IsDeleted)
                    .IsRequired()
                    .HasDefaultValue(false);

                account.Property(a => a.DeletedAt);

                // Indexes for AccountInfo
                account.HasIndex(a => a.IsActive);
                account.HasIndex(a => a.IsDeleted);
                account.HasIndex(a => a.CreatedAt);
                account.HasIndex(a => a.LastLoginAt);
            });
        }

        private void ConfigureSecurityInfo(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.OwnsOne(u => u.Security, security =>
            {
                security.Property(s => s.TwoFactorEnabled)
                    .IsRequired()
                    .HasDefaultValue(false);

                security.Property(s => s.TwoFactorSecret)
                    .HasMaxLength(1000);

                security.Property(s => s.TwoFactorEnabledAt);

                // Indexes for SecurityInfo
                security.HasIndex(s => s.TwoFactorEnabled);
            });
        }

        private void ConfigureAuditInfo(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.OwnsOne(u => u.Audit, audit =>
            {
                audit.Property(a => a.CreatedBy)
                    .HasMaxLength(450);

                audit.Property(a => a.UpdatedBy)
                    .HasMaxLength(450);

                audit.Property(a => a.UpdatedAt);

                // Indexes for AuditInfo
                audit.HasIndex(a => a.CreatedBy);
                audit.HasIndex(a => a.UpdatedAt);
            });
        }

        private void ConfigureProperties(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Basic Identity properties
            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.NormalizedUserName)
                .HasMaxLength(256);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.NormalizedEmail)
                .HasMaxLength(256);

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(50);

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(4000);

            builder.Property(u => u.SecurityStamp)
                .HasMaxLength(4000);

            builder.Property(u => u.ConcurrencyStamp)
                .HasMaxLength(4000);

            // Custom properties
            builder.Property(u => u.CustomerId)
                .HasMaxLength(450);

            // Default values
            builder.Property(u => u.EmailConfirmed)
                .HasDefaultValue(false);

            builder.Property(u => u.PhoneNumberConfirmed)
                .HasDefaultValue(false);

            builder.Property(u => u.TwoFactorEnabled)
                .HasDefaultValue(false);

            builder.Property(u => u.LockoutEnabled)
                .HasDefaultValue(true);

            builder.Property(u => u.AccessFailedCount)
                .HasDefaultValue(0);
        }

        private void ConfigureIndexes(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Unique indexes
            builder.HasIndex(u => u.UserName)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_UserName");

            builder.HasIndex(u => u.NormalizedUserName)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_NormalizedUserName");

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_Email");

            builder.HasIndex(u => u.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("IX_AspNetUsers_NormalizedEmail");

            // Regular indexes
            builder.HasIndex(u => u.CustomerId)
                .HasDatabaseName("IX_AspNetUsers_CustomerId");

            builder.HasIndex(u => u.EmailConfirmed)
                .HasDatabaseName("IX_AspNetUsers_EmailConfirmed");

            builder.HasIndex(u => u.PhoneNumberConfirmed)
                .HasDatabaseName("IX_AspNetUsers_PhoneNumberConfirmed");

            builder.HasIndex(u => u.TwoFactorEnabled)
                .HasDatabaseName("IX_AspNetUsers_TwoFactorEnabled");

            builder.HasIndex(u => u.LockoutEnabled)
                .HasDatabaseName("IX_AspNetUsers_LockoutEnabled");

            builder.HasIndex(u => u.AccessFailedCount)
                .HasDatabaseName("IX_AspNetUsers_AccessFailedCount");

            builder.HasIndex(u => u.LockoutEnd)
                .HasDatabaseName("IX_AspNetUsers_LockoutEnd");
        }

        private void ConfigureNavigationProperties(EntityTypeBuilder<ApplicationUser> builder)
        {
            // UserClaims relationship
            builder.HasMany(u => u.UserClaims)
                .WithOne()
                .HasForeignKey(uc => uc.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // UserLogins relationship
            builder.HasMany(u => u.UserLogins)
                .WithOne()
                .HasForeignKey(ul => ul.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // UserRoles relationship
            builder.HasMany(u => u.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // UserTokens relationship
            builder.HasMany(u => u.UserTokens)
                .WithOne()
                .HasForeignKey(ut => ut.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
