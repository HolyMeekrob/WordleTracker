using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordleTracker.Data.Migrations
{
    public partial class AddGroupUpdatedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdatedDate",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Groups");
        }
    }
}
