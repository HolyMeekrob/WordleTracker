using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordleTracker.Data.Migrations;

public partial class ManuallySetDayId : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<int>(
			name: "Id",
			table: "Days",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER")
			.OldAnnotation("Sqlite:Autoincrement", true);
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<int>(
			name: "Id",
			table: "Days",
			type: "INTEGER",
			nullable: false,
			oldClrType: typeof(int),
			oldType: "INTEGER")
			.Annotation("Sqlite:Autoincrement", true);
	}
}
