using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGistIndexToPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,");

            // Вместо AlterColumn — сырой SQL с USING
            migrationBuilder.Sql(
                "ALTER TABLE departments ALTER COLUMN path TYPE ltree USING path::ltree");

            migrationBuilder.CreateIndex(
                    name: "idx_departments_path",
                    table: "departments",
                    column: "path")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_departments_path",
                table: "departments");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "path",
                table: "departments",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "ltree",
                oldMaxLength: 1000);
        }
    }
}
