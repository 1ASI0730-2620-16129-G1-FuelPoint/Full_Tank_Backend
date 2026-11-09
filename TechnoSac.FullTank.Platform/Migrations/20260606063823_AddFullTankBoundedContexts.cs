using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace TechnoSac.FullTank.Platform.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTankBoundedContexts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deliveries",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    driver_id = table.Column<int>(type: "int", nullable: true),
                    vehicle_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    origin_location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    destination_location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    dispatched_at = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    delivered_at = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_deliveries", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "drivers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    license_number = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    phone = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    email = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_drivers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "equipment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    required_fuel_type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    current_level = table.Column<int>(type: "int", nullable: false),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    favorite_provider_id = table.Column<int>(type: "int", nullable: true),
                    auto_refill = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    refill_threshold = table.Column<int>(type: "int", nullable: false),
                    location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    last_refill_date = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_equipment", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "favorite_providers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    provider_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_favorite_providers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inventory_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    price_per_liter = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    stock = table.Column<int>(type: "int", nullable: false),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    low_stock_threshold = table.Column<int>(type: "int", nullable: false),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_inventory_items", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inventory_movements",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    inventory_item_id = table.Column<int>(type: "int", nullable: true),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_inventory_movements", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    payment_id = table.Column<int>(type: "int", nullable: true),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    invoice_number = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    provider_ruc = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    provider_name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    buyer_ruc = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    buyer_name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    fuel_type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    igv = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    issue_date = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_invoices", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    recipient_type = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    buyer_company_id = table.Column<int>(type: "int", nullable: true),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    title = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    is_read = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    related_id = table.Column<int>(type: "int", nullable: true),
                    target_route = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_notifications", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    request_id = table.Column<int>(type: "int", nullable: true),
                    buyer_company_id = table.Column<int>(type: "int", nullable: true),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    equipment_id = table.Column<int>(type: "int", nullable: true),
                    fuel_type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    delivery_address = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    payment_status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    driver_id = table.Column<int>(type: "int", nullable: true),
                    vehicle_id = table.Column<int>(type: "int", nullable: true),
                    estimated_delivery_date = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    dispatched_at = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    delivered_at = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    paid_at = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    closed_at = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    cancelled_at = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    cancel_reason = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_orders", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    company_id = table.Column<int>(type: "int", nullable: true),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    method = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    masked_card = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    card_holder = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    reference = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_payments", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "provider_products",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    fuel_type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false),
                    price_per_liter = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    available = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_provider_products", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "refill_histories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    equipment_id = table.Column<int>(type: "int", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: true),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    fuel_type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    request_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_refill_histories", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    buyer_company_id = table.Column<int>(type: "int", nullable: true),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    equipment_id = table.Column<int>(type: "int", nullable: true),
                    fuel_type = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    product_name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    delivery_address = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false),
                    delivery_date = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    source = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    rejection_reason_code = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true),
                    rejection_reason_note = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_requests", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    plate = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    brand = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    model = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_vehicles", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deliveries");

            migrationBuilder.DropTable(
                name: "drivers");

            migrationBuilder.DropTable(
                name: "equipment");

            migrationBuilder.DropTable(
                name: "favorite_providers");

            migrationBuilder.DropTable(
                name: "inventory_items");

            migrationBuilder.DropTable(
                name: "inventory_movements");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "provider_products");

            migrationBuilder.DropTable(
                name: "refill_histories");

            migrationBuilder.DropTable(
                name: "requests");

            migrationBuilder.DropTable(
                name: "vehicles");
        }
    }
}
