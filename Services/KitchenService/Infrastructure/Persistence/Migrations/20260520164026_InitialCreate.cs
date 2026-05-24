using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoPulse.KitchenService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KitchenTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TableNo = table.Column<int>(type: "int", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrepStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadyAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BumpedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitchenTickets", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KitchenTickets");
        }
    }
}
