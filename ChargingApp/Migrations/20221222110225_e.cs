using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class e : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "ChangerAndCompanies");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RechargeCodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "RechargeCodes");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodId",
                table: "ChangerAndCompanies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
