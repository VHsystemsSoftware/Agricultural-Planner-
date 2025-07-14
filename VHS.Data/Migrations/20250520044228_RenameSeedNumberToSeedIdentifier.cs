using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class RenameSeedNumberToSeedIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeedNumber",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "SeedIdentifier",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeedIdentifier",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "SeedNumber",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
