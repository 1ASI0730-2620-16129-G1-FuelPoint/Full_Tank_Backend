using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoSac.FullTank.Platform.Migrations
{
    /// <inheritdoc />
    public partial class CompleteFavoritesAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "reserved",
                table: "inventory_items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "i_x_favorite_providers_company_id_provider_id",
                table: "favorite_providers",
                columns: new[] { "company_id", "provider_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_favorite_providers_company_id_provider_id",
                table: "favorite_providers");

            migrationBuilder.DropColumn(
                name: "reserved",
                table: "inventory_items");
        }
    }
}
