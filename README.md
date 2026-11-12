# FullTank Platform - Backend API

**AV2 / Render:** [Deployment configuration, MySQL, demo mode and verification](docs/RENDER_AV2.md).
The AV2 Blueprint runs Production with explicitly enabled academic simulation;
payments do not process money and receipts are not SUNAT electronic invoices.

Modular Monolith backend for **FullTank**, built with **ASP.NET Core (.NET 10)**, **Entity Framework Core**, **MySQL 8.0+**, and **Domain-Driven Design (DDD)** principles.

---

## 1. Architecture Overview & Design Boundaries

The system is organized into autonomous bounded contexts interacting via **Anti-Corruption Layer (ACL) Facades** and in-process mediator commands and domain events:

- **IAM (Identity & Access Management)**: Authentication, BCrypt password hashing, JWT token issuance, and company-bound onboarding (`BUYER` and `PROVIDER`).
- **Equipment**: Buyer equipment registry (generators, industrial machinery, storage tanks) with fuel capacity and auto-refill thresholds.
- **Catalog**: Fuel product catalogs, provider pricing, unit conversions (L/GAL), and provider ratings.
- **Inventory**: Provider fuel storage tanks, concurrency-guarded stock levels, reservations, and inventory movements (`IN`/`OUT`).
- **Ordering**: Buyer fuel requests and confirmed order state lifecycle (`PENDING` -> `ACCEPTED` -> `DISPATCHED` -> `PENDING_PAYMENT` -> `PAID` / `CLOSED` / `CANCELLED`).
- **Fulfillment**: Fleet drivers and tanker vehicles assigned to order dispatches.
- **Payment**: Authoritative checkout orchestration, deterministic simulated receipts, and Peru IGV (18%) financial calculations.
- **Notification**: Cross-boundary domain notifications and asynchronous event logging.
- **Reporting & Analytics**: Read-only spending and operational analytics dashboards.

### Architecture Scope & Disclaimers
- **In-Process Events**: Domain events (`IEventBus`) decouple logging, notification dispatch, and bounded context coordination within the application process. There is **no transactional outbox** or distributed cross-process message broker; this is an academic modular monolith rather than a distributed microservices platform.
- **Testing Scope**: The 67 automated test cases mock repositories, facades, and controller contexts. Tests validate business logic and security boundaries in memory; they do **not** simulate live multi-connection MySQL concurrency or network-level E2E latency.
- **Simulated Operations**: Dispatches and receipts are simulated workflows. The platform does not integrate real-time GPS telemetry or legal electronic invoicing (SUNAT).

---

## 2. Physical Quantity Model & Unit Conversion

To avoid floating-point drift in storage and financial records, fuel quantities are modeled with specific integer conventions:

- **Integer Quantities**: Physical quantities (`Stock`, `Reserved`, `Capacity`, `CurrentLevel`, and order `Quantity`) are stored as integers in the database.
- **Conservative Ceiling Conversion**: Fuel requests created in Gallons (`GAL`) are converted to inventory stock units (Liters) using conservative ceiling rounding via `FuelUnitConverter.ConvertToStockUnitsCeil`:
  $$\text{StockUnits} = \lceil \text{Quantity}_{\text{gal}} \times 3.78541 \rceil$$
  This ensures providers never under-reserve stock due to fractional gallon-to-liter calculations.
- **Equipment Level Rounding**: Refills and level updates use integer math capped at tank capacity (`Math.Min(Capacity, CurrentLevel + quantity)`). Equipment monitoring reflects discrete integer volume increments rather than arbitrary decimal precision.

---

## 3. Configuration & Security Precedence

Configuration follows a layered precedence hierarchy:
1. `appsettings.json`: Base configuration with empty placeholders and strict production defaults.
2. `appsettings.Development.json`: Development overrides (e.g., `PaymentSettings:EnableSimulation: true`).
3. `appsettings.Local.json`: Developer-specific local settings. Loaded **strictly in Development** (`builder.Environment.IsDevelopment()`); gitignored; excluded from publish output (`CopyToPublishDirectory="Never"`). Preserves developer credentials.
4. **Environment Variables & CLI Args**: Loaded last in `Program.cs` (`AddEnvironmentVariables()` and `AddCommandLine(args)`), taking final precedence over JSON files.

### Production Hardening
- **Explicit CORS**: Outside Development, startup requires explicit HTTPS origins without paths or trailing slashes. The AV2 Blueprint sets both Firebase origins.
- **Placeholder Fail-Fast**: Production startup immediately rejects unresolved `%DATABASE_%` or `%JWT_%` placeholders.
- **Simulation Gate**: Outside Development, simulated payments require both `PaymentSettings:EnableSimulation=true` and the explicit `AcademicDemo:Enabled=true` opt-in. They never process money.
- **Publish Safety**: Local configuration files (`appsettings.Local.json`) are excluded from build publish outputs.

### Company Registration Token Contract
To prevent unauthorized company association and account takeover:
- `POST /api/v1/buyer-companies` and `POST /api/v1/provider-companies` return an authenticated, purpose-bound `registrationToken` (24-hour expiration via ASP.NET Core Data Protection).
- `POST /api/v1/users/sign-up` requires `registrationToken` alongside `companyId` and `role`. Registration validates the cryptographic token under a `Serializable` database transaction, enforcing that only one public owner account can register per company and role.

---

## 4. Agreed Frontend Workflow Contract

The core ordering lifecycle transitions are orchestrated in `OrderCommandService` under `Serializable` unit-of-work transactions:

1. **Approve Request** (`POST /api/v1/fuel-requests/{id}/approve`): Provider approves request; authoritative price is pulled from `InventoryItem`; stock is reserved; order transitions to `ACCEPTED`.
2. **Dispatch Order** (`POST /api/v1/orders/{id}/dispatch`): Provider assigns available driver and vehicle; reserved stock is deducted with an `OUT` movement; order transitions to `DISPATCHED`.
3. **Receive Order** (`POST /api/v1/orders/{id}/receive`): Buyer confirms delivery; driver and vehicle are released; target equipment is refilled; order transitions to `PENDING_PAYMENT`.
4. **Cancel Order** (`POST /api/v1/orders/{id}/cancel`): Owning provider cancels an undispatched `ACCEPTED` order; reserved inventory is released; order transitions to `CANCELLED`.

> [!NOTE]
> Direct `POST /api/v1/orders` creation and generic `PUT /api/v1/orders/{id}` updates are disabled (`400 Bad Request`) to protect lifecycle invariants. Active orders cannot be deleted via `DELETE`.

---

## 5. Authoritative Invoicing & Payment Checkout (Simulated)

The `POST /api/v1/payment-checkout` endpoint provides atomic payment checkout:
- **Simulation Only**: Academic simulation without live banking or SUNAT electronic invoice compliance. Checkouts generate deterministic receipts (`SIM-INV-{orderId:D8}`) and payment reference `SIM-{orderId:D8}`.
- **Authoritative Data Derivation**: Client-supplied financial and party fields are discarded. Quantities, fuel types, and prices are resolved from `IOrderingContextFacade`; company names and RUCs are fetched from `IIamContextFacade`.
- **Peruvian IGV Math (18%)**:
  $$\text{Subtotal} = \text{Round}\left(\frac{\text{Total}}{1.18}, 2\right), \quad \text{IGV} = \text{Total} - \text{Subtotal}$$
- **Atomic Rollback**: Payments, invoices, order status (`PAID`), and provider notifications commit atomically inside a `Serializable` transaction; failures at any step trigger a complete rollback.
- **BOLA Enforcement**: The caller's authenticated company must match the order's buyer company (`ActorCompanyId == orderBuyerCompanyId`). Legacy direct creation endpoints on `/api/v1/payments` and `/api/v1/invoices` are disabled.

---

## 6. Database Migrations & Preflight Verification

### Migration Summary
EF Core Migration `20260908173116_EnforceIamAndInvoiceIntegrity` adds two essential unique database indexes:
1. `i_x_users_company_id_role` on `users(company_id, role)`: Enforces one public owner account per company/role (multi-user employee invitations are not implemented).
2. `i_x_invoices_invoice_number` on `invoices(invoice_number)`: Enforces uniqueness for invoice numbers.

> [!IMPORTANT]
> Production defaults keep `Database:ApplyMigrationsOnStartup=false`. The AV2 Blueprint explicitly enables migrations on its dedicated database. Migrations and seeding have been tested against disposable local MySQL; this does not establish that any public database is deployed or migrated. Review existing data and take a backup before enabling migrations on a database you need to preserve.

### Preflight SQL Duplicate Check (Mandatory Before Migration)
Before applying the migration, administrators **must back up the database** and run the following queries to verify no duplicate records exist. Any duplicate rows **must be reviewed and resolved manually**; no automatic deletion is performed.

```sql
-- 1. Check for duplicate accounts per company and role
SELECT company_id, role, COUNT(*) AS duplicate_count
FROM users
WHERE company_id IS NOT NULL
GROUP BY company_id, role
HAVING COUNT(*) > 1;

-- 2. Check for duplicate invoice numbers
SELECT invoice_number, COUNT(*) AS duplicate_count
FROM invoices
WHERE invoice_number IS NOT NULL
GROUP BY invoice_number
HAVING COUNT(*) > 1;
```

---

## 7. Local Setup & Execution Guide

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [MySQL 8.0+ Server](https://dev.mysql.com/downloads/installer/)

### 1. Install Entity Framework Core CLI Tools
From the `backend-main` root directory, install `dotnet-ef` 10.0.8 globally (or configure a local tool manifest):
```bash
dotnet tool install --global dotnet-ef --version 10.0.8
```
*(If already installed, verify with `dotnet ef --version`.)*

### 2. Configure Local Settings
Create or update `TechnoSac.FullTank.Platform/appsettings.Local.json` (this file is gitignored; existing local files are preserved):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=fulltank;Uid=root;Pwd=your_local_password;"
  },
  "TokenSettings": {
    "Secret": "your_secure_random_jwt_secret_key_minimum_32_characters_long"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://127.0.0.1:5173"]
  }
}
```

### 3. Apply Database Migrations
After running the preflight duplicate SQL checks, execute the pending migration from `backend-main`:
```bash
dotnet ef database update --project TechnoSac.FullTank.Platform -- --environment Development
```

### 4. Build and Run Tests
```bash
# Build entire solution
dotnet build full-tank-platform.sln

# Run all 67 unit and security boundary tests
dotnet test full-tank-platform.sln --logger "console;verbosity=normal"
```

### 5. Launch the API
```bash
dotnet run --project TechnoSac.FullTank.Platform/TechnoSac.FullTank.Platform.csproj
```

Swagger UI will be available at:
- `http://localhost:5204/swagger`
- `https://localhost:7018/swagger`
