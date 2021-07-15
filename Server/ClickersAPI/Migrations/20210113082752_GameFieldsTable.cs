using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class GameFieldsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AccountDataFields");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "AccountDataFields");

            migrationBuilder.CreateTable(
                name: "GameDataFields",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(nullable: false),
                    FieldId = table.Column<ushort>(nullable: false),
                    LastUpdate = table.Column<DateTime>(nullable: false),
                    NumericValue = table.Column<long>(nullable: true),
                    StringValue = table.Column<string>(nullable: true),
                    BoolValue = table.Column<bool>(nullable: true),
                    FloatingValue = table.Column<decimal>(nullable: true),
                    DateTimeValue = table.Column<DateTime>(nullable: true),
                    GameId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameDataFields", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameDataFields");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AccountDataFields",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "AccountDataFields",
                type: "int",
                nullable: true);
        }
    }
}
