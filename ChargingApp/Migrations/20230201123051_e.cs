using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class e : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ApiProducts_ProductId",
                table: "ApiProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiOrders_OrderId",
                table: "ApiOrders",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiOrders_Orders_OrderId",
                table: "ApiOrders",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApiProducts_Products_ProductId",
                table: "ApiProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiOrders_Orders_OrderId",
                table: "ApiOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ApiProducts_Products_ProductId",
                table: "ApiProducts");

            migrationBuilder.DropIndex(
                name: "IX_ApiProducts_ProductId",
                table: "ApiProducts");

            migrationBuilder.DropIndex(
                name: "IX_ApiOrders_OrderId",
                table: "ApiOrders");
        }
    }
}
