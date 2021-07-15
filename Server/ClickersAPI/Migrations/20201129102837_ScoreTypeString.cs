using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class ScoreTypeString : Migration
    {
        protected override void Up(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Scores",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Scores",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
