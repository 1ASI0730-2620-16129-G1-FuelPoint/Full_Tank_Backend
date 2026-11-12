using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoSac.FullTank.Platform.Migrations
{
    /// <inheritdoc />
    public partial class EnforcePaymentIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "i_x_payments_order_id",
                table: "payments",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_invoices_order_id",
                table: "invoices",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_invoices_payment_id",
                table: "invoices",
                column: "payment_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_payments_order_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "i_x_invoices_order_id",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "i_x_invoices_payment_id",
                table: "invoices");
        }
    }
}
