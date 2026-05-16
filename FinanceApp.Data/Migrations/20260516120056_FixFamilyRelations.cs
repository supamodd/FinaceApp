using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixFamilyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Families_Users_CreatedById",
                table: "Families");

            migrationBuilder.DropIndex(
                name: "IX_Families_CreatedById",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Families");

            migrationBuilder.CreateTable(
                name: "FamilyInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FamilyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyInvitations_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Families_CreatedByUserId",
                table: "Families",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyInvitations_FamilyId",
                table: "FamilyInvitations",
                column: "FamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Users_CreatedByUserId",
                table: "Families",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Families_Users_CreatedByUserId",
                table: "Families");

            migrationBuilder.DropTable(
                name: "FamilyInvitations");

            migrationBuilder.DropIndex(
                name: "IX_Families_CreatedByUserId",
                table: "Families");

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Families",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Families_CreatedById",
                table: "Families",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Users_CreatedById",
                table: "Families",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
