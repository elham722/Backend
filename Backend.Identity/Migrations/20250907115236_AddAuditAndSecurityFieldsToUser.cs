using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditAndSecurityFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Audit_CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "Audit_LastModifiedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Audit_LastModifiedBy",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Security_LastSecurityUpdate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Security_SecurityAnswer",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Security_SecurityQuestion",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Audit_CreatedAt",
                table: "AspNetUsers",
                column: "Audit_CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Audit_LastModifiedAt",
                table: "AspNetUsers",
                column: "Audit_LastModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Security_LastSecurityUpdate",
                table: "AspNetUsers",
                column: "Security_LastSecurityUpdate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Audit_CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Audit_LastModifiedAt",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Security_LastSecurityUpdate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Audit_CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Audit_LastModifiedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Audit_LastModifiedBy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Security_LastSecurityUpdate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Security_SecurityAnswer",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Security_SecurityQuestion",
                table: "AspNetUsers");
        }
    }
}
