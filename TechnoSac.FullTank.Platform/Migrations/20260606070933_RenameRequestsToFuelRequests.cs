using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoSac.FullTank.Platform.Migrations
{
    /// <inheritdoc />
    public partial class RenameRequestsToFuelRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "p_k_requests",
                table: "requests");

            migrationBuilder.RenameTable(
                name: "requests",
                newName: "fuel_requests");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_fuel_requests",
                table: "fuel_requests",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "p_k_fuel_requests",
                table: "fuel_requests");

            migrationBuilder.RenameTable(
                name: "fuel_requests",
                newName: "requests");

            migrationBuilder.AddPrimaryKey(
                name: "p_k_requests",
                table: "requests",
                column: "id");
        }
    }
}
