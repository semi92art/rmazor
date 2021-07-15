using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class NoOptionsListInProfile : Migration
    {
        protected override void Up(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.DropColumn(
                name: "Option1",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option10",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option10Available",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option1Available",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option2",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option2Available",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option3",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option3Available",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option4",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option4Available",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option5",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option5Available",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option6",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option6Available",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option7",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option7Available",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option8",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option8Available",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option9",
                table: "Profiles");

            _MigrationBuilder.DropColumn(
                name: "Option9Available",
                table: "Profiles");

            _MigrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateTime",
                table: "Scores",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.DropColumn(
                name: "LastUpdateTime",
                table: "Scores");

            _MigrationBuilder.AddColumn<int>(
                name: "Option1",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<int>(
                name: "Option10",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option10Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option1Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _MigrationBuilder.AddColumn<int>(
                name: "Option2",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option2Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _MigrationBuilder.AddColumn<int>(
                name: "Option3",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option3Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _MigrationBuilder.AddColumn<int>(
                name: "Option4",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option4Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _MigrationBuilder.AddColumn<int>(
                name: "Option5",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option5Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _MigrationBuilder.AddColumn<int>(
                name: "Option6",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option6Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _MigrationBuilder.AddColumn<int>(
                name: "Option7",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option7Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _MigrationBuilder.AddColumn<int>(
                name: "Option8",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option8Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            _MigrationBuilder.AddColumn<int>(
                name: "Option9",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            _MigrationBuilder.AddColumn<bool>(
                name: "Option9Available",
                table: "Profiles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
