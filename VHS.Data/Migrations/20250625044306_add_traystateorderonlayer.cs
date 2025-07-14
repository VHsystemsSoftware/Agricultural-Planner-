using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_traystateorderonlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GrowOrderOnLayer",
                table: "TrayStates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreGrowOrderOnLayer",
                table: "TrayStates",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrowOrderOnLayer",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "PreGrowOrderOnLayer",
                table: "TrayStates");
        }
    }
}
