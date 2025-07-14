using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeJobTraysTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecipeId",
                table: "JobTrays",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "JobTrays");
        }
    }
}
