using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class JobRelatedUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "BatchPlans");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "BatchPlans");

            migrationBuilder.RenameColumn(
                name: "TotalTrays",
                table: "BatchPlans",
                newName: "DaysForPlan");

            migrationBuilder.AlterColumn<int>(
                name: "OrderOnLayer",
                table: "JobTrays",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "JobId1",
                table: "JobTrays",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "BatchPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobTrays_JobId1",
                table: "JobTrays",
                column: "JobId1");

            migrationBuilder.CreateIndex(
                name: "IX_JobTrays_ParentJobTrayId",
                table: "JobTrays",
                column: "ParentJobTrayId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTrays_JobTrays_ParentJobTrayId",
                table: "JobTrays",
                column: "ParentJobTrayId",
                principalTable: "JobTrays",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTrays_Jobs_JobId1",
                table: "JobTrays",
                column: "JobId1",
                principalTable: "Jobs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTrays_JobTrays_ParentJobTrayId",
                table: "JobTrays");

            migrationBuilder.DropForeignKey(
                name: "FK_JobTrays_Jobs_JobId1",
                table: "JobTrays");

            migrationBuilder.DropIndex(
                name: "IX_JobTrays_JobId1",
                table: "JobTrays");

            migrationBuilder.DropIndex(
                name: "IX_JobTrays_ParentJobTrayId",
                table: "JobTrays");

            migrationBuilder.DropColumn(
                name: "JobId1",
                table: "JobTrays");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "BatchPlans");

            migrationBuilder.RenameColumn(
                name: "DaysForPlan",
                table: "BatchPlans",
                newName: "TotalTrays");

            migrationBuilder.AlterColumn<int>(
                name: "OrderOnLayer",
                table: "JobTrays",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "BatchPlans",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "BatchPlans",
                type: "time",
                nullable: true);
        }
    }
}
