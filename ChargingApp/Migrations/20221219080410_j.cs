using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class j : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Scucced",
                table: "Payments",
                newName: "Succeed");

            migrationBuilder.RenameColumn(
                name: "Aproved",
                table: "Payments",
                newName: "Approved");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Succeed",
                table: "Payments",
                newName: "Scucced");

            migrationBuilder.RenameColumn(
                name: "Approved",
                table: "Payments",
                newName: "Aproved");
        }
    }
}
