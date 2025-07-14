using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_transportlayerid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrowTransportLayerId",
                table: "TrayStates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreGrowTransportLayerId",
                table: "TrayStates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransportLayerId",
                table: "JobTrays",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrowTransportLayerId",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "PreGrowTransportLayerId",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "TransportLayerId",
                table: "JobTrays");
        }
    }
}
