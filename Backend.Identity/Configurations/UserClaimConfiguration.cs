using Backend.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Identity.Configurations
{
    public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
    {
        public void Configure(EntityTypeBuilder<UserClaim> builder)
        {
            // Table name
            builder.ToTable("AspNetUserClaims");

            // Primary key
            builder.HasKey(uc => uc.Id);

            // Properties
            builder.Property(uc => uc.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(uc => uc.ClaimType)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(uc => uc.ClaimValue)
                .HasMaxLength(4000);

            builder.Property(uc => uc.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(uc => uc.CreatedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(uc => uc.UpdatedBy)
                .HasMaxLength(450);

            builder.Property(uc => uc.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(uc => uc.UserId)
                .HasDatabaseName("IX_AspNetUserClaims_UserId");

            builder.HasIndex(uc => uc.IsActive)
                .HasDatabaseName("IX_AspNetUserClaims_IsActive");

            builder.HasIndex(uc => uc.CreatedAt)
                .HasDatabaseName("IX_AspNetUserClaims_CreatedAt");

            builder.HasIndex(uc => uc.CreatedBy)
                .HasDatabaseName("IX_AspNetUserClaims_CreatedBy");

            // Composite indexes
            builder.HasIndex(uc => new { uc.UserId, uc.IsActive })
                .HasDatabaseName("IX_AspNetUserClaims_UserId_IsActive");

            builder.HasIndex(uc => new { uc.ClaimType, uc.ClaimValue })
                .HasDatabaseName("IX_AspNetUserClaims_ClaimType_ClaimValue");
        }
    }
}
