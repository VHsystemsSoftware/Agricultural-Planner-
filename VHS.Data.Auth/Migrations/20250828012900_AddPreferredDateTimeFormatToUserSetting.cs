using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Auth.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredDateTimeFormatToUserSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredDateTimeFormat",
                table: "UserSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "dd-MM-yyyy HH:mm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredDateTimeFormat",
                table: "UserSettings");
        }
    }
}
