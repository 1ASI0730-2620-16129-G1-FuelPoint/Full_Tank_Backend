using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnoSac.FullTank.Platform.Migrations
{
    /// <inheritdoc />
    public partial class EnforceIamAndInvoiceIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "i_x_users_company_id_role",
                table: "users",
                columns: new[] { "company_id", "role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_invoices_invoice_number",
                table: "invoices",
                column: "invoice_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "i_x_users_company_id_role",
                table: "users");

            migrationBuilder.DropIndex(
                name: "i_x_invoices_invoice_number",
                table: "invoices");
        }
    }
}
