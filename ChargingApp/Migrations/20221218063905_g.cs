using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class g : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinimunPurchasingAllowed",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimunPurchasingAllowed",
                table: "Products");
        }
    }
}
