using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class h : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinimunPurchasingAllowed",
                table: "Products",
                newName: "MinimumQuantityAllowed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinimumQuantityAllowed",
                table: "Products",
                newName: "MinimunPurchasingAllowed");
        }
    }
}
