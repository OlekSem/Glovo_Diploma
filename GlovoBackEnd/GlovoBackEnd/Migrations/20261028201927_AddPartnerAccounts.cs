using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GlovoBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartnerRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmationCodeHash = table.Column<string>(type: "text", nullable: true),
                    ConfirmationCodeExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordResetTokenHash = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartnerUserRoles",
                columns: table => new
                {
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerUserRoles", x => new { x.PartnerId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_PartnerUserRoles_PartnerRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "PartnerRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerUserRoles_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PartnerRoles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "CompanyOwner" },
                    { 2, "AffiliateManager" },
                    { 3, "AffiliateEmployee" },
                    { 4, "User" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartnerRoles_Name",
                table: "PartnerRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_NormalizedEmail",
                table: "Partners",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_Phone",
                table: "Partners",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerUserRoles_RoleId",
                table: "PartnerUserRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartnerUserRoles");

            migrationBuilder.DropTable(
                name: "PartnerRoles");

            migrationBuilder.DropTable(
                name: "Partners");
        }
    }
}
