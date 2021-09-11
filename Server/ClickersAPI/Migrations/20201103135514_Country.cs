using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class Country : Migration
    {
        protected override void Up(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.AddColumn<string>(
                name: "CountryKey",
                table: "Accounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.DropColumn(
                name: "CountryKey",
                table: "Accounts");
        }
    }
}
