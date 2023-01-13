using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class v : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanceledOrders");

            migrationBuilder.RenameColumn(
                name: "Cancel",
                table: "Orders",
                newName: "StatusIfCanceled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StatusIfCanceled",
                table: "Orders",
                newName: "Cancel");

            migrationBuilder.CreateTable(
                name: "CanceledOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    AdminMessage = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanceledOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanceledOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CanceledOrders_OrderId",
                table: "CanceledOrders",
                column: "OrderId");
        }
    }
}
