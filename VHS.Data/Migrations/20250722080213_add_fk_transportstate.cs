using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_fk_transportstate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TrayStates_GrowTransportLayerId",
                table: "TrayStates",
                column: "GrowTransportLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayStates_PreGrowTransportLayerId",
                table: "TrayStates",
                column: "PreGrowTransportLayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrayStates_Layers_GrowTransportLayerId",
                table: "TrayStates",
                column: "GrowTransportLayerId",
                principalTable: "Layers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrayStates_Layers_PreGrowTransportLayerId",
                table: "TrayStates",
                column: "PreGrowTransportLayerId",
                principalTable: "Layers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrayStates_Layers_GrowTransportLayerId",
                table: "TrayStates");

            migrationBuilder.DropForeignKey(
                name: "FK_TrayStates_Layers_PreGrowTransportLayerId",
                table: "TrayStates");

            migrationBuilder.DropIndex(
                name: "IX_TrayStates_GrowTransportLayerId",
                table: "TrayStates");

            migrationBuilder.DropIndex(
                name: "IX_TrayStates_PreGrowTransportLayerId",
                table: "TrayStates");
        }
    }
}
