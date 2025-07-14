using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class split_databases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OPCAudits");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OPCAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageInput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageInputDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageOutput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageOutputDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiveDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OPCAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Auth0Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PreferredLengthUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredMeasurementSystem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredTemperatureUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredTheme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredVolumeUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredWeightUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Auth0Id",
                table: "Users",
                column: "Auth0Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);
        }
    }
}
