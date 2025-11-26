using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_layerbatchid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "Layers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTransportLayer",
                table: "Layers",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "IsTransportLayer",
                table: "Layers");
        }
    }
}
