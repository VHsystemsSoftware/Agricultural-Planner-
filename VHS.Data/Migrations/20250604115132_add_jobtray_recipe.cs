using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_jobtray_recipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JobTrays_RecipeId",
                table: "JobTrays",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTrays_Recipes_RecipeId",
                table: "JobTrays",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTrays_Recipes_RecipeId",
                table: "JobTrays");

            migrationBuilder.DropIndex(
                name: "IX_JobTrays_RecipeId",
                table: "JobTrays");
        }
    }
}
