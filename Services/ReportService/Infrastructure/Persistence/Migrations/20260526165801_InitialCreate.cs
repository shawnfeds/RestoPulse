using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoPulse.ReportService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemSales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    TableNo = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Revenues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TableId = table.Column<int>(type: "int", nullable: false),
                    TableNo = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SettledAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revenues", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemSales_MenuItemId",
                table: "ItemSales",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSales_OrderedAt",
                table: "ItemSales",
                column: "OrderedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSales_OrderNo",
                table: "ItemSales",
                column: "OrderNo");

            migrationBuilder.CreateIndex(
                name: "IX_Revenues_BillNo",
                table: "Revenues",
                column: "BillNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Revenues_OrderNo",
                table: "Revenues",
                column: "OrderNo");

            migrationBuilder.CreateIndex(
                name: "IX_Revenues_SettledAt",
                table: "Revenues",
                column: "SettledAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemSales");

            migrationBuilder.DropTable(
                name: "Revenues");
        }
    }
}
