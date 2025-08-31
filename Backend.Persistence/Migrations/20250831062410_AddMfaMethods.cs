using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMfaMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MfaMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuditInfo_CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AuditInfo_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditInfo_ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AuditInfo_ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuditInfo_IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    AuditInfo_UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotpSecretKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotpQrCodeUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotpDigits = table.Column<int>(type: "int", nullable: false, defaultValue: 6),
                    TotpPeriod = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SmsCodeExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSmsCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    BackupCodes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemainingBackupCodes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfaMethods", x => x.Id);
                    table.CheckConstraint("CK_MfaMethods_FailedAttempts", "FailedAttempts >= 0");
                    table.CheckConstraint("CK_MfaMethods_RemainingBackupCodes", "RemainingBackupCodes >= 0");
                    table.CheckConstraint("CK_MfaMethods_TotpDigits", "TotpDigits BETWEEN 4 AND 10");
                    table.CheckConstraint("CK_MfaMethods_TotpPeriod", "TotpPeriod BETWEEN 15 AND 60");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MfaMethods_IsEnabled",
                table: "MfaMethods",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_MfaMethods_Type",
                table: "MfaMethods",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_MfaMethods_UserId",
                table: "MfaMethods",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MfaMethods_UserId_Type",
                table: "MfaMethods",
                columns: new[] { "UserId", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MfaMethods");
        }
    }
}
