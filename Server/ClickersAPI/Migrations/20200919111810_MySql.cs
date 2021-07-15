using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ClickersAPI.Migrations
{
    public partial class MySql : Migration
    {
        protected override void Up(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.CreateTable(
                name: "Accounts",
                columns: _Table => new
                {
                    Id = _Table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = _Table.Column<string>(nullable: true),
                    PasswordHash = _Table.Column<string>(nullable: true),
                    DeviceId = _Table.Column<string>(nullable: true),
                    CreationTime = _Table.Column<DateTime>(nullable: false)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_Accounts", _X => _X.Id);
                });

            _MigrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: _Table => new
                {
                    Id = _Table.Column<string>(nullable: false),
                    Name = _Table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = _Table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = _Table.Column<string>(nullable: true)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_AspNetRoles", _X => _X.Id);
                });

            _MigrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: _Table => new
                {
                    Id = _Table.Column<string>(nullable: false),
                    UserName = _Table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = _Table.Column<string>(maxLength: 256, nullable: true),
                    Email = _Table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = _Table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = _Table.Column<bool>(nullable: false),
                    PasswordHash = _Table.Column<string>(nullable: true),
                    SecurityStamp = _Table.Column<string>(nullable: true),
                    ConcurrencyStamp = _Table.Column<string>(nullable: true),
                    PhoneNumber = _Table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = _Table.Column<bool>(nullable: false),
                    TwoFactorEnabled = _Table.Column<bool>(nullable: false),
                    LockoutEnd = _Table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = _Table.Column<bool>(nullable: false),
                    AccessFailedCount = _Table.Column<int>(nullable: false)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_AspNetUsers", _X => _X.Id);
                });

            _MigrationBuilder.CreateTable(
                name: "Profiles",
                columns: _Table => new
                {
                    Id = _Table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = _Table.Column<int>(nullable: false),
                    GameId = _Table.Column<int>(nullable: false),
                    CreationTime = _Table.Column<DateTime>(nullable: false),
                    Option1 = _Table.Column<int>(nullable: false),
                    Option1Available = _Table.Column<bool>(nullable: false),
                    Option2 = _Table.Column<int>(nullable: false),
                    Option2Available = _Table.Column<bool>(nullable: false),
                    Option3 = _Table.Column<int>(nullable: false),
                    Option3Available = _Table.Column<bool>(nullable: false),
                    Option4 = _Table.Column<int>(nullable: false),
                    Option4Available = _Table.Column<bool>(nullable: false),
                    Option5 = _Table.Column<int>(nullable: false),
                    Option5Available = _Table.Column<bool>(nullable: false),
                    Option6 = _Table.Column<int>(nullable: false),
                    Option6Available = _Table.Column<bool>(nullable: false),
                    Option7 = _Table.Column<int>(nullable: false),
                    Option7Available = _Table.Column<bool>(nullable: false),
                    Option8 = _Table.Column<int>(nullable: false),
                    Option8Available = _Table.Column<bool>(nullable: false),
                    Option9 = _Table.Column<int>(nullable: false),
                    Option9Available = _Table.Column<bool>(nullable: false),
                    Option10 = _Table.Column<int>(nullable: false),
                    Option10Available = _Table.Column<bool>(nullable: false)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_Profiles", _X => _X.Id);
                });

            _MigrationBuilder.CreateTable(
                name: "Scores",
                columns: _Table => new
                {
                    Id = _Table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = _Table.Column<int>(nullable: false),
                    GameId = _Table.Column<int>(nullable: false),
                    CreationTime = _Table.Column<DateTime>(nullable: false),
                    Type = _Table.Column<int>(nullable: false),
                    Points = _Table.Column<int>(nullable: false)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_Scores", _X => _X.Id);
                });

            _MigrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: _Table => new
                {
                    Id = _Table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = _Table.Column<string>(nullable: false),
                    ClaimType = _Table.Column<string>(nullable: true),
                    ClaimValue = _Table.Column<string>(nullable: true)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_AspNetRoleClaims", _X => _X.Id);
                    _Table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: _X => _X.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _MigrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: _Table => new
                {
                    Id = _Table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = _Table.Column<string>(nullable: false),
                    ClaimType = _Table.Column<string>(nullable: true),
                    ClaimValue = _Table.Column<string>(nullable: true)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_AspNetUserClaims", _X => _X.Id);
                    _Table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: _X => _X.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _MigrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: _Table => new
                {
                    LoginProvider = _Table.Column<string>(nullable: false),
                    ProviderKey = _Table.Column<string>(nullable: false),
                    ProviderDisplayName = _Table.Column<string>(nullable: true),
                    UserId = _Table.Column<string>(nullable: false)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_AspNetUserLogins", _X => new { _X.LoginProvider, _X.ProviderKey });
                    _Table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: _X => _X.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _MigrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: _Table => new
                {
                    UserId = _Table.Column<string>(nullable: false),
                    RoleId = _Table.Column<string>(nullable: false)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_AspNetUserRoles", _X => new { _X.UserId, _X.RoleId });
                    _Table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: _X => _X.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _Table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: _X => _X.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _MigrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: _Table => new
                {
                    UserId = _Table.Column<string>(nullable: false),
                    LoginProvider = _Table.Column<string>(nullable: false),
                    Name = _Table.Column<string>(nullable: false),
                    Value = _Table.Column<string>(nullable: true)
                },
                constraints: _Table =>
                {
                    _Table.PrimaryKey("PK_AspNetUserTokens", _X => new { _X.UserId, _X.LoginProvider, _X.Name });
                    _Table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: _X => _X.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _MigrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            _MigrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            _MigrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            _MigrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            _MigrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            _MigrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            _MigrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);
        }

        protected override void Down(MigrationBuilder _MigrationBuilder)
        {
            _MigrationBuilder.DropTable(
                name: "Accounts");

            _MigrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            _MigrationBuilder.DropTable(
                name: "AspNetUserClaims");

            _MigrationBuilder.DropTable(
                name: "AspNetUserLogins");

            _MigrationBuilder.DropTable(
                name: "AspNetUserRoles");

            _MigrationBuilder.DropTable(
                name: "AspNetUserTokens");

            _MigrationBuilder.DropTable(
                name: "Profiles");

            _MigrationBuilder.DropTable(
                name: "Scores");

            _MigrationBuilder.DropTable(
                name: "AspNetRoles");

            _MigrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
