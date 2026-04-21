using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ix_locations_name 
                ON locations (name);
            
                CREATE UNIQUE INDEX ix_locations_address 
                ON locations (address_country, address_city, address_street, address_building, address_office);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ix_locations_name;
                DROP INDEX IF EXISTS ix_locations_address;
            ");
        }
    }
}
