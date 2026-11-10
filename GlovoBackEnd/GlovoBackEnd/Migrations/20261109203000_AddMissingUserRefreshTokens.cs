using GlovoBackEnd.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlovoBackEnd.Migrations;

/// <summary>
/// Repairs the original identity migration, which omitted the refresh-token
/// properties already present in <see cref="Domain.Entities.Identity.UserEntity"/>.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20261109203000_AddMissingUserRefreshTokens")]
public partial class AddMissingUserRefreshTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // IF NOT EXISTS keeps the migration safe for databases that received
        // these columns outside EF migrations.
        migrationBuilder.Sql("""
            ALTER TABLE "AspNetUsers"
                ADD COLUMN IF NOT EXISTS "RefreshToken" text NULL,
                ADD COLUMN IF NOT EXISTS "RefreshTokenExpiry" timestamp with time zone NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "RefreshToken", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "RefreshTokenExpiry", table: "AspNetUsers");
    }
}
