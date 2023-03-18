using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HammadBroker.Data.Migrations
{
    public partial class buildingEnabled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "Buildings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "Buildings");
        }
    }
}
