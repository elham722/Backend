using Backend.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Identity.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            // Table name
            builder.ToTable("AuditLogs");

            // Primary key
            builder.HasKey(al => al.Id);

            // Properties Configuration
            builder.Property(al => al.Id)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(al => al.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(al => al.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(al => al.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(al => al.EntityId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(al => al.OldValues)
                .HasColumnType("nvarchar(max)");

            builder.Property(al => al.NewValues)
                .HasColumnType("nvarchar(max)");

            builder.Property(al => al.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(al => al.IpAddress)
                .IsRequired()
                .HasMaxLength(45);

            builder.Property(al => al.UserAgent)
                .HasMaxLength(1000);

            builder.Property(al => al.DeviceInfo)
                .HasMaxLength(500);

            builder.Property(al => al.SessionId)
                .HasMaxLength(450);

            builder.Property(al => al.RequestId)
                .HasMaxLength(450);

            builder.Property(al => al.AdditionalData)
                .HasColumnType("nvarchar(max)");

            builder.Property(al => al.IsSuccess)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(al => al.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(al => al.Severity)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Info");

            // Indexes
            builder.HasIndex(al => al.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            builder.HasIndex(al => al.Action)
                .HasDatabaseName("IX_AuditLogs_Action");

            builder.HasIndex(al => al.EntityType)
                .HasDatabaseName("IX_AuditLogs_EntityType");

            builder.HasIndex(al => al.EntityId)
                .HasDatabaseName("IX_AuditLogs_EntityId");

            builder.HasIndex(al => al.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");

            builder.HasIndex(al => al.IpAddress)
                .HasDatabaseName("IX_AuditLogs_IpAddress");

            builder.HasIndex(al => al.IsSuccess)
                .HasDatabaseName("IX_AuditLogs_IsSuccess");

            builder.HasIndex(al => al.Severity)
                .HasDatabaseName("IX_AuditLogs_Severity");

            builder.HasIndex(al => al.SessionId)
                .HasDatabaseName("IX_AuditLogs_SessionId");

            builder.HasIndex(al => al.RequestId)
                .HasDatabaseName("IX_AuditLogs_RequestId");

            // Composite indexes for common queries
            builder.HasIndex(al => new { al.UserId, al.Timestamp })
                .HasDatabaseName("IX_AuditLogs_UserId_Timestamp");

            builder.HasIndex(al => new { al.EntityType, al.EntityId, al.Timestamp })
                .HasDatabaseName("IX_AuditLogs_EntityType_EntityId_Timestamp");

            builder.HasIndex(al => new { al.Action, al.Timestamp })
                .HasDatabaseName("IX_AuditLogs_Action_Timestamp");
        }
    }
}