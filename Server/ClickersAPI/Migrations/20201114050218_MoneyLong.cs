using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class MoneyLong : Migration
    {
        protected override void Up(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.AlterColumn<long>(
                name: "Gold",
                table: "Profiles",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            _MigrationBuilder.AlterColumn<long>(
                name: "Diamonds",
                table: "Profiles",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.AlterColumn<int>(
                name: "Gold",
                table: "Profiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(long));

            _MigrationBuilder.AlterColumn<int>(
                name: "Diamonds",
                table: "Profiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
