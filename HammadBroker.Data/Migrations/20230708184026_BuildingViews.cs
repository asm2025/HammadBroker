using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HammadBroker.Data.Migrations
{
    public partial class BuildingViews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PageViews",
                table: "Buildings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Views",
                table: "Buildings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageViews",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "Buildings");
        }
    }
}
