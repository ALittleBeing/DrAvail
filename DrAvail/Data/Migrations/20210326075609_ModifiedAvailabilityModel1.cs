using Microsoft.EntityFrameworkCore.Migrations;

namespace DrAvail.Data.Migrations
{
    public partial class ModifiedAvailabilityModel1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityType",
                table: "Availabilities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityType",
                table: "Availabilities");
        }
    }
}
