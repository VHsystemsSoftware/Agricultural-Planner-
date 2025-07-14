using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class fix_columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTrays_Jobs_JobId1",
                table: "JobTrays");

            migrationBuilder.DropForeignKey(
                name: "FK_Layers_Jobs_JobId",
                table: "Layers");

            migrationBuilder.DropIndex(
                name: "IX_Layers_JobId",
                table: "Layers");

            migrationBuilder.DropIndex(
                name: "IX_JobTrays_JobId1",
                table: "JobTrays");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "Layers");

            migrationBuilder.DropColumn(
                name: "JobId1",
                table: "JobTrays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "Layers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JobId1",
                table: "JobTrays",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Layers_JobId",
                table: "Layers",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTrays_JobId1",
                table: "JobTrays",
                column: "JobId1");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTrays_Jobs_JobId1",
                table: "JobTrays",
                column: "JobId1",
                principalTable: "Jobs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Layers_Jobs_JobId",
                table: "Layers",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id");
        }
    }
}
