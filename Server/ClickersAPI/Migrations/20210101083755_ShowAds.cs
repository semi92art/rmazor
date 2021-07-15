using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class ShowAds : Migration
    {
        protected override void Up(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.AddColumn<bool>(
                "ShowAds",
                "Profiles",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.DropColumn(
                "ShowAds",
                "Profiles");
        }
    }
}