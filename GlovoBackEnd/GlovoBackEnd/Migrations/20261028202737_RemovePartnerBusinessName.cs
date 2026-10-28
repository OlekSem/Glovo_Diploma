using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlovoBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class RemovePartnerBusinessName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "Partners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "Partners",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");
        }
    }
}
