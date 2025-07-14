using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FarmTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmTypes", x => x.Id);
                });

			var now = DateTime.UtcNow;
			migrationBuilder.InsertData(
				table: "FarmTypes",
				columns: new[] { "Id", "Name", "Description", "AddedDateTime", "DeletedDateTime" },
				values: new object[,]
				{
					{ Guid.NewGuid(), "Vertical", "Vertical farm", now, null },
					{ Guid.NewGuid(), "Horizontal", "Horizontal farm", now, null },
					{ Guid.NewGuid(), "Container", "Container farm", now, null }
				});

			migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Auth0Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Farms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FarmTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Farms_FarmTypes_FarmTypeId",
                        column: x => x.FarmTypeId,
                        principalTable: "FarmTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PreferredTheme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredMeasurementSystem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredWeightUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredLengthUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredTemperatureUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredVolumeUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Floors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FloorNumber = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Floors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Floors_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LightZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TargetDLI = table.Column<double>(type: "float", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LightZones_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeedNumber = table.Column<int>(type: "int", nullable: false),
                    SeedSupplier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Trays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trays_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WaterZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TargetDWR = table.Column<double>(type: "float", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterZones_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Racks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LayerCount = table.Column<int>(type: "int", nullable: false),
                    TrayCountPerLayer = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Racks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Racks_Floors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "Floors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LightZoneSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LightZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Intensity = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CalculatedDLI = table.Column<double>(type: "float", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightZoneSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LightZoneSchedules_LightZones_LightZoneId",
                        column: x => x.LightZoneId,
                        principalTable: "LightZones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GerminationDays = table.Column<int>(type: "int", nullable: false),
                    PropagationDays = table.Column<int>(type: "int", nullable: false),
                    GrowDays = table.Column<int>(type: "int", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrayTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromPhaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToPhaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "WaterZoneSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WaterZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    VolumeUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CalculatedDWR = table.Column<double>(type: "float", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterZoneSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterZoneSchedules_WaterZones_WaterZoneId",
                        column: x => x.WaterZoneId,
                        principalTable: "WaterZones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BatchPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalTrays = table.Column<int>(type: "int", nullable: false),
                    TraysPerDay = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchPlans_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BatchPlans_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecipeLightSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LightZoneScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetDLI = table.Column<double>(type: "float", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeLightSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeLightSchedules_LightZoneSchedules_LightZoneScheduleId",
                        column: x => x.LightZoneScheduleId,
                        principalTable: "LightZoneSchedules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecipeLightSchedules_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecipeWaterSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WaterZoneScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetDWR = table.Column<double>(type: "float", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeWaterSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeWaterSchedules_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecipeWaterSchedules_WaterZoneSchedules_WaterZoneScheduleId",
                        column: x => x.WaterZoneScheduleId,
                        principalTable: "WaterZoneSchedules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrayCount = table.Column<int>(type: "int", nullable: false),
                    SeedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HarvestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batches_BatchPlans_BatchPlanId",
                        column: x => x.BatchPlanId,
                        principalTable: "BatchPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Batches_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderOnDay = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrayCount = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JobLocationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Batches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Layers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LayerNumber = table.Column<int>(type: "int", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Layers_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Layers_Racks_RackId",
                        column: x => x.RackId,
                        principalTable: "Racks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobTrays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrayId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentJobTrayId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinationLocation = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationLayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderOnLayer = table.Column<int>(type: "int", nullable: false),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTrays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobTrays_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JobTrays_Layers_DestinationLayerId",
                        column: x => x.DestinationLayerId,
                        principalTable: "Layers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JobTrays_Trays_TrayId",
                        column: x => x.TrayId,
                        principalTable: "Trays",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrayCurrentStates",
                columns: table => new
                {
                    TrayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationLayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderOnLayer = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentPhaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeededDateTimeUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_Batches_BatchPlanId",
                table: "Batches",
                column: "BatchPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_FarmId",
                table: "Batches",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchPlans_FarmId",
                table: "BatchPlans",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchPlans_RecipeId",
                table: "BatchPlans",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Farms_FarmTypeId",
                table: "Farms",
                column: "FarmTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Floors_FarmId",
                table: "Floors",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_BatchId",
                table: "Jobs",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTrays_DestinationLayerId",
                table: "JobTrays",
                column: "DestinationLayerId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTrays_JobId",
                table: "JobTrays",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTrays_TrayId",
                table: "JobTrays",
                column: "TrayId");

            migrationBuilder.CreateIndex(
                name: "IX_Layers_JobId",
                table: "Layers",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Layers_RackId",
                table: "Layers",
                column: "RackId");

            migrationBuilder.CreateIndex(
                name: "IX_LightZones_FarmId",
                table: "LightZones",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_LightZoneSchedules_LightZoneId",
                table: "LightZoneSchedules",
                column: "LightZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_FarmId",
                table: "Products",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Racks_FloorId",
                table: "Racks",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLightSchedules_LightZoneScheduleId",
                table: "RecipeLightSchedules",
                column: "LightZoneScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLightSchedules_RecipeId",
                table: "RecipeLightSchedules",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_ProductId",
                table: "Recipes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeWaterSchedules_RecipeId",
                table: "RecipeWaterSchedules",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeWaterSchedules_WaterZoneScheduleId",
                table: "RecipeWaterSchedules",
                column: "WaterZoneScheduleId");

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
                name: "IX_Trays_FarmId",
                table: "Trays",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_TrayTransactions_TrayId",
                table: "TrayTransactions",
                column: "TrayId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Auth0Id",
                table: "Users",
                column: "Auth0Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaterZones_FarmId",
                table: "WaterZones",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_WaterZoneSchedules_WaterZoneId",
                table: "WaterZoneSchedules",
                column: "WaterZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobTrays");

            migrationBuilder.DropTable(
                name: "RecipeLightSchedules");

            migrationBuilder.DropTable(
                name: "RecipeWaterSchedules");

            migrationBuilder.DropTable(
                name: "TrayCurrentStates");

            migrationBuilder.DropTable(
                name: "TrayTransactions");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "LightZoneSchedules");

            migrationBuilder.DropTable(
                name: "WaterZoneSchedules");

            migrationBuilder.DropTable(
                name: "Layers");

            migrationBuilder.DropTable(
                name: "Trays");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LightZones");

            migrationBuilder.DropTable(
                name: "WaterZones");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Racks");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropTable(
                name: "Floors");

            migrationBuilder.DropTable(
                name: "BatchPlans");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Farms");

            migrationBuilder.DropTable(
                name: "FarmTypes");
        }
    }
}
