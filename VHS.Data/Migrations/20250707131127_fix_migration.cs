using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class fix_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobTypeId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobTypeId",
                table: "Jobs");
        }
    }
}
