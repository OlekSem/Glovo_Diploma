using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlovoBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModerationComment",
                table: "Companies",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedById",
                table: "Companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Companies",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Pending");

            // Companies created before moderation existed remain active after upgrade.
            // The database default remains Pending for every new submission.
            migrationBuilder.Sql("UPDATE \"Companies\" SET \"Status\" = 'Approved' WHERE \"Status\" = 'Pending';");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Status",
                table: "Companies",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_Status",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ModerationComment",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Companies");
        }
    }
}
