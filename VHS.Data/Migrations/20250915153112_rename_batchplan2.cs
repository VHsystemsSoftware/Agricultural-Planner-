using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class rename_batchplan2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_BatchPlans_GrowPlanId",
                table: "Batches");

            migrationBuilder.DropForeignKey(
                name: "FK_BatchPlans_Farms_FarmId",
                table: "BatchPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_BatchPlans_Recipes_RecipeId",
                table: "BatchPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BatchPlans",
                table: "BatchPlans");

            migrationBuilder.RenameTable(
                name: "BatchPlans",
                newName: "GrowPlans");

            migrationBuilder.RenameColumn(
                name: "BatchPlanTypeId",
                table: "GrowPlans",
                newName: "GrowPlanTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_BatchPlans_RecipeId",
                table: "GrowPlans",
                newName: "IX_GrowPlans_RecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_BatchPlans_FarmId",
                table: "GrowPlans",
                newName: "IX_GrowPlans_FarmId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GrowPlans",
                table: "GrowPlans",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_GrowPlans_GrowPlanId",
                table: "Batches",
                column: "GrowPlanId",
                principalTable: "GrowPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GrowPlans_Farms_FarmId",
                table: "GrowPlans",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GrowPlans_Recipes_RecipeId",
                table: "GrowPlans",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_GrowPlans_GrowPlanId",
                table: "Batches");

            migrationBuilder.DropForeignKey(
                name: "FK_GrowPlans_Farms_FarmId",
                table: "GrowPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_GrowPlans_Recipes_RecipeId",
                table: "GrowPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GrowPlans",
                table: "GrowPlans");

            migrationBuilder.RenameTable(
                name: "GrowPlans",
                newName: "BatchPlans");

            migrationBuilder.RenameColumn(
                name: "GrowPlanTypeId",
                table: "BatchPlans",
                newName: "BatchPlanTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_GrowPlans_RecipeId",
                table: "BatchPlans",
                newName: "IX_BatchPlans_RecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_GrowPlans_FarmId",
                table: "BatchPlans",
                newName: "IX_BatchPlans_FarmId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BatchPlans",
                table: "BatchPlans",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_BatchPlans_GrowPlanId",
                table: "Batches",
                column: "GrowPlanId",
                principalTable: "BatchPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchPlans_Farms_FarmId",
                table: "BatchPlans",
                column: "FarmId",
                principalTable: "Farms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchPlans_Recipes_RecipeId",
                table: "BatchPlans",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }
    }
}
