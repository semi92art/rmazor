using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class RemoveCountry : Migration
    {
        protected override void Up(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.DropColumn(
                name: "CountryKey",
                table: "Accounts");
        }

        protected override void Down(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.AddColumn<string>(
                name: "CountryKey",
                table: "Accounts",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
