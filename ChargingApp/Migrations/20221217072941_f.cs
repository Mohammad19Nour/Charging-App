using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class f : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RechargeType",
                table: "Payments",
                newName: "PaymentType");

            migrationBuilder.RenameColumn(
                name: "PaymentGateway",
                table: "Payments",
                newName: "PaymentAgent");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentType",
                table: "Payments",
                newName: "RechargeType");

            migrationBuilder.RenameColumn(
                name: "PaymentAgent",
                table: "Payments",
                newName: "PaymentGateway");
        }
    }
}
