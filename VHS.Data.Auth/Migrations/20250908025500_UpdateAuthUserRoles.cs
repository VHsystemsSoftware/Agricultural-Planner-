using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHS.Data.Auth.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuthUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'farm_manager')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES (NEWID(), 'farm_manager', 'FARM_MANAGER', NEWID())
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'operator')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES (NEWID(), 'operator', 'OPERATOR', NEWID())
                END");

            migrationBuilder.Sql(@"
                UPDATE AspNetUserRoles 
                SET RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'farm_manager')
                WHERE RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'user')");

            migrationBuilder.Sql(@"
                DELETE FROM AspNetRoles WHERE Name = 'user'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'user')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES (NEWID(), 'user', 'USER', NEWID())
                END");

            migrationBuilder.Sql(@"
                UPDATE AspNetUserRoles 
                SET RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'user')
                WHERE RoleId IN (
                    SELECT Id FROM AspNetRoles WHERE Name IN ('farm_manager', 'operator')
                )");

            migrationBuilder.Sql(@"
                DELETE FROM AspNetRoles WHERE Name IN ('farm_manager', 'operator')");
        }
    }
}
