using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SlugWasChangedToDowninvariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "departments",
                newName: "slug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "slug",
                table: "departments",
                newName: "Slug");
        }
    }
}
