using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class rename_batchplan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_BatchPlans_BatchPlanId",
                table: "Batches");

            migrationBuilder.RenameColumn(
                name: "BatchPlanId",
                table: "Batches",
                newName: "GrowPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Batches_BatchPlanId",
                table: "Batches",
                newName: "IX_Batches_GrowPlanId");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "StartDate",
                table: "BatchPlans",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "SeedDate",
                table: "Batches",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "HarvestDate",
                table: "Batches",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_BatchPlans_GrowPlanId",
                table: "Batches",
                column: "GrowPlanId",
                principalTable: "BatchPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_BatchPlans_GrowPlanId",
                table: "Batches");

            migrationBuilder.RenameColumn(
                name: "GrowPlanId",
                table: "Batches",
                newName: "BatchPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Batches_GrowPlanId",
                table: "Batches",
                newName: "IX_Batches_BatchPlanId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "BatchPlans",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SeedDate",
                table: "Batches",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "HarvestDate",
                table: "Batches",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_BatchPlans_BatchPlanId",
                table: "Batches",
                column: "BatchPlanId",
                principalTable: "BatchPlans",
                principalColumn: "Id");
        }
    }
}
