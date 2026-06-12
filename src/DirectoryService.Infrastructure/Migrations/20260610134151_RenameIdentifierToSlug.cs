using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameIdentifierToSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_departments_departments_DepartmentId",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "IX_departments_DepartmentId",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "identifier",
                table: "departments");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "departments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "department_locations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "department_locations");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "identifier",
                table: "departments",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_departments_DepartmentId",
                table: "departments",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_departments_departments_DepartmentId",
                table: "departments",
                column: "DepartmentId",
                principalTable: "departments",
                principalColumn: "id");
        }
    }
}
