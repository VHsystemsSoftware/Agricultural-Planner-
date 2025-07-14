using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_racknumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LayerNumber",
                table: "Layers",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "FloorNumber",
                table: "Floors",
                newName: "Number");

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Racks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Number",
                table: "Racks");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Layers",
                newName: "LayerNumber");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Floors",
                newName: "FloorNumber");
        }
    }
}
