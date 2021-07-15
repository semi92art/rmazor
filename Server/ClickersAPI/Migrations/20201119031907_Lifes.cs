using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class Lifes : Migration
    {
        protected override void Up(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.AddColumn<long>(
                name: "Lifes",
                table: "Profiles",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.DropColumn(
                name: "Lifes",
                table: "Profiles");
        }
    }
}
