using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace GlovoBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "CompanyBranches",
                type: "geography(Point,4326)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_Location",
                table: "CompanyBranches",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyBranches_Location",
                table: "CompanyBranches");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "CompanyBranches");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
