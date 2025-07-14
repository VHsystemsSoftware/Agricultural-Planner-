using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class BatchPlanRowsToBatchRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchPlanRows");

            migrationBuilder.CreateTable(
                name: "BatchRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RackId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchRows_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BatchRows_Floors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "Floors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BatchRows_Layers_LayerId",
                        column: x => x.LayerId,
                        principalTable: "Layers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BatchRows_Racks_RackId",
                        column: x => x.RackId,
                        principalTable: "Racks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchRows_BatchId",
                table: "BatchRows",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchRows_FloorId",
                table: "BatchRows",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchRows_LayerId",
                table: "BatchRows",
                column: "LayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchRows_RackId",
                table: "BatchRows",
                column: "RackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchRows");

            migrationBuilder.CreateTable(
                name: "BatchPlanRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RackId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchPlanRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchPlanRows_BatchPlans_BatchPlanId",
                        column: x => x.BatchPlanId,
                        principalTable: "BatchPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BatchPlanRows_Floors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "Floors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BatchPlanRows_Layers_LayerId",
                        column: x => x.LayerId,
                        principalTable: "Layers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BatchPlanRows_Racks_RackId",
                        column: x => x.RackId,
                        principalTable: "Racks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchPlanRows_BatchPlanId",
                table: "BatchPlanRows",
                column: "BatchPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchPlanRows_FloorId",
                table: "BatchPlanRows",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchPlanRows_LayerId",
                table: "BatchPlanRows",
                column: "LayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchPlanRows_RackId",
                table: "BatchPlanRows",
                column: "RackId");
        }
    }
}
