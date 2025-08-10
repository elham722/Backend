using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                CustomerStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsPremium = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                EntityStatus = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Active")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Customers_CreatedAt",
            table: "Customers",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_CustomerStatus",
            table: "Customers",
            column: "CustomerStatus");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_CustomerStatus_IsVerified",
            table: "Customers",
            columns: new[] { "CustomerStatus", "IsVerified" });

        migrationBuilder.CreateIndex(
            name: "IX_Customers_DateOfBirth",
            table: "Customers",
            column: "DateOfBirth");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_Email",
            table: "Customers",
            column: "Email",
            unique: true,
            filter: "[Email] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_FirstName",
            table: "Customers",
            column: "FirstName");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_FirstName_LastName",
            table: "Customers",
            columns: new[] { "FirstName", "LastName" });

        migrationBuilder.CreateIndex(
            name: "IX_Customers_IsPremium",
            table: "Customers",
            column: "IsPremium");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_IsPremium_CustomerStatus",
            table: "Customers",
            columns: new[] { "IsPremium", "CustomerStatus" });

        migrationBuilder.CreateIndex(
            name: "IX_Customers_IsVerified",
            table: "Customers",
            column: "IsVerified");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_LastName",
            table: "Customers",
            column: "LastName");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_PhoneNumber",
            table: "Customers",
            column: "PhoneNumber",
            unique: true,
            filter: "[PhoneNumber] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Customers");
    }
} 