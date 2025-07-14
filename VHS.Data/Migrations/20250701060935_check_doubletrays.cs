using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class check_doubletrays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrayStates_Trays_TrayId",
                table: "TrayStates");

            migrationBuilder.CreateIndex(
                name: "IX_Trays_Tag",
                table: "Trays",
                column: "Tag",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TrayStates_Trays_TrayId",
                table: "TrayStates",
                column: "TrayId",
                principalTable: "Trays",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrayStates_Trays_TrayId",
                table: "TrayStates");

            migrationBuilder.DropIndex(
                name: "IX_Trays_Tag",
                table: "Trays");

            migrationBuilder.AddForeignKey(
                name: "FK_TrayStates_Trays_TrayId",
                table: "TrayStates",
                column: "TrayId",
                principalTable: "Trays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
