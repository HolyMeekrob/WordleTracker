using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordleTracker.Data.Migrations
{
    public partial class RecognizeHardMode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HardMode",
                table: "Results",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HardMode",
                table: "Results");
        }
    }
}
