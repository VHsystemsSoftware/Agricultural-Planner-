using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_opcaudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OPCAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    MessageInput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageOutput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiveDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageInputDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageOutputDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OPCAudits", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OPCAudits");
        }
    }
}
