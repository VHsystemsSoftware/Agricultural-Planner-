using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_batchplan_fks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BatchPlanRows_FloorId",
                table: "BatchPlanRows",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchPlanRows_LayerId",
                table: "BatchPlanRows",
                column: "LayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchPlanRows_RackId",
                table: "BatchPlanRows",
                column: "RackId");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchPlanRows_Floors_FloorId",
                table: "BatchPlanRows",
                column: "FloorId",
                principalTable: "Floors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchPlanRows_Layers_LayerId",
                table: "BatchPlanRows",
                column: "LayerId",
                principalTable: "Layers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchPlanRows_Racks_RackId",
                table: "BatchPlanRows",
                column: "RackId",
                principalTable: "Racks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchPlanRows_Floors_FloorId",
                table: "BatchPlanRows");

            migrationBuilder.DropForeignKey(
                name: "FK_BatchPlanRows_Layers_LayerId",
                table: "BatchPlanRows");

            migrationBuilder.DropForeignKey(
                name: "FK_BatchPlanRows_Racks_RackId",
                table: "BatchPlanRows");

            migrationBuilder.DropIndex(
                name: "IX_BatchPlanRows_FloorId",
                table: "BatchPlanRows");

            migrationBuilder.DropIndex(
                name: "IX_BatchPlanRows_LayerId",
                table: "BatchPlanRows");

            migrationBuilder.DropIndex(
                name: "IX_BatchPlanRows_RackId",
                table: "BatchPlanRows");
        }
    }
}
