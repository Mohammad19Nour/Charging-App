using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class c : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Photos_ReceiptPhotoId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ReceiptPhotoId",
                table: "Orders",
                newName: "PhotoId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_ReceiptPhotoId",
                table: "Orders",
                newName: "IX_Orders_PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Photos_PhotoId",
                table: "Orders",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Photos_PhotoId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "PhotoId",
                table: "Orders",
                newName: "ReceiptPhotoId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_PhotoId",
                table: "Orders",
                newName: "IX_Orders_ReceiptPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Photos_ReceiptPhotoId",
                table: "Orders",
                column: "ReceiptPhotoId",
                principalTable: "Photos",
                principalColumn: "Id");
        }
    }
}
