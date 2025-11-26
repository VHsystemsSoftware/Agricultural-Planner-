using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_bufferlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBufferLayer",
                table: "Layers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("update layers set IsBufferLayer=0\r\nupdate layers set IsBufferLayer=1 from racks r where r.id = layers.RackId and r.LayerCount-1 = layers.Number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBufferLayer",
                table: "Layers");
        }
    }
}
