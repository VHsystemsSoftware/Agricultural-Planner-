using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Audit.Migrations
{
    /// <inheritdoc />
    public partial class add_audit_columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrayTag",
                table: "OPCAudits",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrayTag",
                table: "OPCAudits");
        }
    }
}
