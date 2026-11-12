using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoSac.FullTank.Platform.Migrations
{
    /// <inheritdoc />
    public partial class EnforceOrderingLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "i_x_orders_request_id",
                table: "orders",
                column: "request_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_orders_request_id",
                table: "orders");
        }
    }
}
