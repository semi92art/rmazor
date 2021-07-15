using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class DataFieldValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Scores");

            migrationBuilder.DropColumn(
                name: "AnalyticId",
                table: "FieldValues");

            migrationBuilder.AlterColumn<long>(
                name: "NumericValue",
                table: "FieldValues",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<bool>(
                name: "BoolValue",
                table: "FieldValues",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTimeValue",
                table: "FieldValues",
                nullable: true);

            migrationBuilder.AddColumn<ushort>(
                name: "FieldId",
                table: "FieldValues",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<decimal>(
                name: "FloatingValue",
                table: "FieldValues",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "FieldValues",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdate",
                table: "FieldValues",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTimeValue",
                table: "FieldValues");

            migrationBuilder.DropColumn(
                name: "FieldId",
                table: "FieldValues");

            migrationBuilder.DropColumn(
                name: "FloatingValue",
                table: "FieldValues");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "FieldValues");

            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "FieldValues");

            migrationBuilder.AlterColumn<long>(
                name: "NumericValue",
                table: "FieldValues",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "BoolValue",
                table: "FieldValues",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnalyticId",
                table: "FieldValues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Diamonds = table.Column<long>(type: "bigint", nullable: false),
                    Gold = table.Column<long>(type: "bigint", nullable: false),
                    Lifes = table.Column<long>(type: "bigint", nullable: false),
                    ShowAds = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.Id);
                });
        }
    }
}
