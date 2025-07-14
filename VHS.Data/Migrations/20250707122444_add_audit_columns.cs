using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_audit_columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrowPushedOutByTrayId",
                table: "TrayStates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreGrowPushedOutByTrayId",
                table: "TrayStates",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrowPushedOutByTrayId",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "PreGrowPushedOutByTrayId",
                table: "TrayStates");
        }
    }
}
