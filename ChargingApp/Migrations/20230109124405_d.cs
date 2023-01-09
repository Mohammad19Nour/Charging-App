using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class d : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Photos_ReceiptPhotoId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "ReceiptPhotoId",
                table: "Payments",
                newName: "PhotoId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_ReceiptPhotoId",
                table: "Payments",
                newName: "IX_Payments_PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Photos_PhotoId",
                table: "Payments",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Photos_PhotoId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "PhotoId",
                table: "Payments",
                newName: "ReceiptPhotoId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_PhotoId",
                table: "Payments",
                newName: "IX_Payments_ReceiptPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Photos_ReceiptPhotoId",
                table: "Payments",
                column: "ReceiptPhotoId",
                principalTable: "Photos",
                principalColumn: "Id");
        }
    }
}
