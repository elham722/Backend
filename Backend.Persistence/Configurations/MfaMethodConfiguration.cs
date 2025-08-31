using Backend.Domain.Entities.MFA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for MfaMethod
/// </summary>
public class MfaMethodConfiguration : IEntityTypeConfiguration<MfaMethod>
{
    public void Configure(EntityTypeBuilder<MfaMethod> builder)
    {
        // Table name
        builder.ToTable("MfaMethods");

        // Primary key
        builder.HasKey(m => m.Id);

        // Properties
        builder.Property(m => m.UserId)
            .IsRequired()
            .HasMaxLength(450); // Max length for Identity user ID

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(m => m.IsEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.LastUsed)
            .IsRequired(false);

        builder.Property(m => m.FailedAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(m => m.LockedUntil)
            .IsRequired(false);

        // TOTP specific properties
        builder.Property(m => m.TotpSecretKey)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(m => m.TotpQrCodeUrl)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(m => m.TotpDigits)
            .IsRequired()
            .HasDefaultValue(6);

        builder.Property(m => m.TotpPeriod)
            .IsRequired()
            .HasDefaultValue(30);

        // SMS specific properties
        builder.Property(m => m.PhoneNumber)
            .IsRequired(false)
            .HasMaxLength(20);

        builder.Property(m => m.SmsCodeExpiry)
            .IsRequired(false);

        builder.Property(m => m.LastSmsCode)
            .IsRequired(false)
            .HasMaxLength(10);

        // Backup codes - stored as JSON
        builder.Property(m => m.BackupCodes)
            .IsRequired(false)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
            );

        builder.Property(m => m.RemainingBackupCodes)
            .IsRequired()
            .HasDefaultValue(0);

        // Audit info - owned entity
        builder.OwnsOne(m => m.AuditInfo, audit =>
        {
            audit.Property(a => a.CreatedBy).HasMaxLength(450);
            audit.Property(a => a.CreatedAt).IsRequired();
            audit.Property(a => a.ModifiedBy).HasMaxLength(450);
            audit.Property(a => a.ModifiedAt).IsRequired(false);
            audit.Property(a => a.IpAddress).HasMaxLength(45); // IPv6 max length
            audit.Property(a => a.UserAgent).HasMaxLength(500);
        });

        // Indexes
        builder.HasIndex(m => new { m.UserId, m.Type })
            .IsUnique();

        builder.HasIndex(m => m.UserId)
            .HasDatabaseName("IX_MfaMethods_UserId");

        builder.HasIndex(m => m.Type)
            .HasDatabaseName("IX_MfaMethods_Type");

        builder.HasIndex(m => m.IsEnabled)
            .HasDatabaseName("IX_MfaMethods_IsEnabled");

        // Constraints
        builder.HasCheckConstraint("CK_MfaMethods_TotpDigits", "TotpDigits BETWEEN 4 AND 10");
        builder.HasCheckConstraint("CK_MfaMethods_TotpPeriod", "TotpPeriod BETWEEN 15 AND 60");
        builder.HasCheckConstraint("CK_MfaMethods_FailedAttempts", "FailedAttempts >= 0");
        builder.HasCheckConstraint("CK_MfaMethods_RemainingBackupCodes", "RemainingBackupCodes >= 0");
    }
} 