using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingApp.Migrations
{
    public partial class t : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SpecificBenefit",
                table: "SpecificBenefit");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SpecificBenefit",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpecificBenefit",
                table: "SpecificBenefit",
                columns: new[] { "ProductId", "VipLevel" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SpecificBenefit",
                table: "SpecificBenefit");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SpecificBenefit",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpecificBenefit",
                table: "SpecificBenefit",
                column: "Id");
        }
    }
}
