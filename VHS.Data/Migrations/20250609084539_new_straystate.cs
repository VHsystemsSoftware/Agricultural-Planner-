using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class new_straystate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrayCurrentStates");

            migrationBuilder.DropTable(
                name: "TrayTransactions");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDateTime",
                table: "Batches",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "TrayStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreGrowLayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GrowLayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SeedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreGrowFinishedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    GrowFinishedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Seeding = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TransportToPreGrow = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ArrivedPreGrow = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TransportToGrow = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ArrivedGrow = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TransportToHarvest = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ArrivedHarvest = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TransportToTransplant = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ArrivedTransplant = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TransportToWashing = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ArrivedWashing = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TransportToPaternosterUp = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ArrivedPaternosterUp = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    TransportToPaternosterDown = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ArrivedPaternosterDown = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrayStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrayStates_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrayStates_Layers_GrowLayerId",
                        column: x => x.GrowLayerId,
                        principalTable: "Layers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrayStates_Layers_PreGrowLayerId",
                        column: x => x.PreGrowLayerId,
                        principalTable: "Layers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrayStates_Trays_TrayId",
                        column: x => x.TrayId,
                        principalTable: "Trays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrayStateAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrayStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OPCAuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrayStateAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrayStateAudits_TrayStates_TrayStateId",
                        column: x => x.TrayStateId,
                        principalTable: "TrayStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrayStateAudits_TrayStateId",
                table: "TrayStateAudits",
                column: "TrayStateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayStates_BatchId",
                table: "TrayStates",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayStates_GrowLayerId",
                table: "TrayStates",
                column: "GrowLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayStates_PreGrowLayerId",
                table: "TrayStates",
                column: "PreGrowLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayStates_TrayId",
                table: "TrayStates",
                column: "TrayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrayStateAudits");

            migrationBuilder.DropTable(
                name: "TrayStates");

            migrationBuilder.DropColumn(
                name: "ScheduledDateTime",
                table: "Batches");

            migrationBuilder.CreateTable(
                name: "TrayCurrentStates",
                columns: table => new
                {
                    TrayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinationLayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentPhaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderOnLayer = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SeededDateTimeUTC = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrayCurrentStates", x => x.TrayId);
                    table.ForeignKey(
                        name: "FK_TrayCurrentStates_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrayCurrentStates_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrayCurrentStates_Layers_DestinationLayerId",
                        column: x => x.DestinationLayerId,
                        principalTable: "Layers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrayCurrentStates_Layers_LayerId",
                        column: x => x.LayerId,
                        principalTable: "Layers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrayCurrentStates_Trays_TrayId",
                        column: x => x.TrayId,
                        principalTable: "Trays",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrayTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromPhaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToPhaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrayTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrayTransactions_Trays_TrayId",
                        column: x => x.TrayId,
                        principalTable: "Trays",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrayCurrentStates_BatchId",
                table: "TrayCurrentStates",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayCurrentStates_DestinationLayerId",
                table: "TrayCurrentStates",
                column: "DestinationLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayCurrentStates_JobId",
                table: "TrayCurrentStates",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayCurrentStates_LayerId",
                table: "TrayCurrentStates",
                column: "LayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayTransactions_TrayId",
                table: "TrayTransactions",
                column: "TrayId");
        }
    }
}
