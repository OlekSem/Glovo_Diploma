using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlovoBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchMenuAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchProductCategories_CompanyBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "CompanyBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchProductCategories_CompanyProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CompanyProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchProducts_CompanyBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "CompanyBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchProducts_CompanyProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "CompanyProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchProductCategories_BranchId_CategoryId",
                table: "BranchProductCategories",
                columns: new[] { "BranchId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchProductCategories_CategoryId",
                table: "BranchProductCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchProducts_BranchId_ProductId",
                table: "BranchProducts",
                columns: new[] { "BranchId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchProducts_ProductId",
                table: "BranchProducts",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchProductCategories");

            migrationBuilder.DropTable(
                name: "BranchProducts");
        }
    }
}
