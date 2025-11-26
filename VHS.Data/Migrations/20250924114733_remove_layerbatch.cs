using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class remove_layerbatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Layers_Batches_BatchId",
                table: "Layers");

            migrationBuilder.DropIndex(
                name: "IX_Layers_BatchId",
                table: "Layers");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Layers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "Layers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Layers_BatchId",
                table: "Layers",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Layers_Batches_BatchId",
                table: "Layers",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id");
        }
    }
}
