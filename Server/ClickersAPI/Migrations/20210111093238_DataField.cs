using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class DataField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldValues");

            migrationBuilder.CreateTable(
                name: "DataFields",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    FieldId = table.Column<ushort>(nullable: false),
                    LastUpdate = table.Column<DateTime>(nullable: false),
                    NumericValue = table.Column<long>(nullable: true),
                    StringValue = table.Column<string>(nullable: true),
                    BoolValue = table.Column<bool>(nullable: true),
                    FloatingValue = table.Column<decimal>(nullable: true),
                    DateTimeValue = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataFields", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataFields");

            migrationBuilder.CreateTable(
                name: "FieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    BoolValue = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FieldId = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    FloatingValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NumericValue = table.Column<long>(type: "bigint", nullable: true),
                    StringValue = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldValues", x => x.Id);
                });
        }
    }
}
