using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class rename_pushedoutcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WillBePushedOut",
                table: "TrayStates",
                newName: "WillBePushedOutPreGrow");

            migrationBuilder.AddColumn<DateTime>(
                name: "WillBePushedOutGrow",
                table: "TrayStates",
                type: "datetime2(7)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WillBePushedOutGrow",
                table: "TrayStates");

            migrationBuilder.RenameColumn(
                name: "WillBePushedOutPreGrow",
                table: "TrayStates",
                newName: "WillBePushedOut");
        }
    }
}
