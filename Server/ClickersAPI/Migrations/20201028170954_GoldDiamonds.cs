using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class GoldDiamonds : Migration
    {
        protected override void Up(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.DropColumn(
                name: "GameId",
                table: "Profiles");

            _MigrationBuilder.AddColumn<int>(
                name: "Diamonds",
                table: "Profiles",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<int>(
                name: "Gold",
                table: "Profiles",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.DropColumn(
                name: "Diamonds",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Gold",
                table: "Profiles");

            _MigrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
