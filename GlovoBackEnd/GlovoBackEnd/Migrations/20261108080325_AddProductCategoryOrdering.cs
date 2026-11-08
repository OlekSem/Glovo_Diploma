using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlovoBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategoryOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "CompanyProductCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "CompanyProductCategories");
        }
    }
}
