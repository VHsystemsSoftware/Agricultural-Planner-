using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_weight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "HarvestedWeightKG",
                table: "TrayStates",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WeightRegistered",
                table: "TrayStates",
                type: "datetime2(7)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HarvestedWeightKG",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "WeightRegistered",
                table: "TrayStates");
        }
    }
}
