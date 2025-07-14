using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class changed_traystate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivedPaternosterDown",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "ArrivedPreGrow",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "ArrivedTransplant",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "TransportToGrow",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "TransportToPaternosterDown",
                table: "TrayStates");

            migrationBuilder.RenameColumn(
                name: "TransportToTransplant",
                table: "TrayStates",
                newName: "PropagationToTransplant");

            migrationBuilder.RenameColumn(
                name: "TransportToPreGrow",
                table: "TrayStates",
                newName: "EmptyToTransplant");

            migrationBuilder.AddColumn<Guid>(
                name: "EmptyReason",
                table: "TrayStates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecipeId",
                table: "TrayStates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrayStates_RecipeId",
                table: "TrayStates",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrayStates_Recipes_RecipeId",
                table: "TrayStates",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrayStates_Recipes_RecipeId",
                table: "TrayStates");

            migrationBuilder.DropIndex(
                name: "IX_TrayStates_RecipeId",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "EmptyReason",
                table: "TrayStates");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "TrayStates");

            migrationBuilder.RenameColumn(
                name: "PropagationToTransplant",
                table: "TrayStates",
                newName: "TransportToTransplant");

            migrationBuilder.RenameColumn(
                name: "EmptyToTransplant",
                table: "TrayStates",
                newName: "TransportToPreGrow");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivedPaternosterDown",
                table: "TrayStates",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivedPreGrow",
                table: "TrayStates",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivedTransplant",
                table: "TrayStates",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransportToGrow",
                table: "TrayStates",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransportToPaternosterDown",
                table: "TrayStates",
                type: "datetime2(7)",
                nullable: true);
        }
    }
}
