using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchPlanRowTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchPlanRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RackId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchPlanRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchPlanRows_BatchPlans_BatchPlanId",
                        column: x => x.BatchPlanId,
                        principalTable: "BatchPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchPlanRows_BatchPlanId",
                table: "BatchPlanRows",
                column: "BatchPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchPlanRows");
        }
    }
}
