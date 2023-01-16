using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class r : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Checked",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Succeed",
                table: "Payments",
                newName: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Payments",
                newName: "Succeed");

            migrationBuilder.AddColumn<bool>(
                name: "Checked",
                table: "Payments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
