using BCrypt.Net;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using NotificationEntity = TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates.Notification;
using TechnoSac.FullTank.Platform.Notification.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;

namespace TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Seeds realistic demo data into the database if the main demo accounts do not already exist.
/// </summary>
public static class DatabaseSeeder
{
    public static void Seed(AppDbContext context, string password = "123456")
    {
        // 1. Check if demo user already exists
        if (context.Set<User>().Any(u => u.Email == "logistics@transportesdelsur.com"))
        {
            return;
        }

        // Roll back the entire seed if a later section fails, allowing a clean retry.
        using var transaction = context.Database.BeginTransaction();

        // 2. Buyer Companies
        var buyer1 = context.Set<BuyerCompany>().FirstOrDefault(b => b.Ruc == "20100111222");
        if (buyer1 is null)
        {
            buyer1 = new BuyerCompany(new CreateBuyerCompanyCommand(
                "Transportes del Sur S.A.",
                "20100111222",
                "TRANSPORT",
                "Av. Industrial 234, Callao, Lima",
                "logistics@transportesdelsur.com",
                "+51 1 555 1001"));
            context.Set<BuyerCompany>().Add(buyer1);
            context.SaveChanges();
        }

        var buyer2 = context.Set<BuyerCompany>().FirstOrDefault(b => b.Ruc == "20100222333");
        if (buyer2 is null)
        {
            buyer2 = new BuyerCompany(new CreateBuyerCompanyCommand(
                "Minería Andina Corp.",
                "20100222333",
                "MINING",
                "Carretera Central Km 45, Huancayo",
                "ops@mineriaandina.com",
                "+51 1 555 1002"));
            context.Set<BuyerCompany>().Add(buyer2);
            context.SaveChanges();
        }

        var buyer3 = context.Set<BuyerCompany>().FirstOrDefault(b => b.Ruc == "20100333444");
        if (buyer3 is null)
        {
            buyer3 = new BuyerCompany(new CreateBuyerCompanyCommand(
                "Constructora Lima E.I.R.L.",
                "20100333444",
                "CONSTRUCTION",
                "Av. La Marina 1200, San Miguel, Lima",
                "obras@constructoralima.com",
                "+51 1 555 1003"));
            context.Set<BuyerCompany>().Add(buyer3);
            context.SaveChanges();
        }

        // 3. Provider Companies
        var provider1 = context.Set<ProviderCompany>().FirstOrDefault(p => p.Ruc == "20512345671");
        if (provider1 is null)
        {
            provider1 = new ProviderCompany(new CreateProviderCompanyCommand(
                "Petro Andes Distribuidora S.A.C.",
                "20512345671",
                "Carretera Panamericana Sur Km 18, Villa El Salvador, Lima",
                "+51 1 555 2001",
                4.8m,
                ["DIESEL", "GASOHOL_90", "GASOHOL_95", "GASOHOL_97", "GLP", "GNV"],
                "Distribuidora líder de combustibles industriales en Lima y provincias."));
            context.Set<ProviderCompany>().Add(provider1);
            context.SaveChanges();
        }

        var provider2 = context.Set<ProviderCompany>().FirstOrDefault(p => p.Ruc == "20498765432");
        if (provider2 is null)
        {
            provider2 = new ProviderCompany(new CreateProviderCompanyCommand(
                "Combustibles del Pacífico S.A.",
                "20498765432",
                "Av. Argentina 4500, Callao, Lima",
                "+51 1 555 2002",
                4.6m,
                ["DIESEL", "GASOHOL_95", "GLP"],
                "Especialistas en suministro marítimo, pesquero e industrial."));
            context.Set<ProviderCompany>().Add(provider2);
            context.SaveChanges();
        }

        var provider3 = context.Set<ProviderCompany>().FirstOrDefault(p => p.Ruc == "20587654321");
        if (provider3 is null)
        {
            provider3 = new ProviderCompany(new CreateProviderCompanyCommand(
                "Energía Total Perú E.I.R.L.",
                "20587654321",
                "Av. Tomás Valle 890, Los Olivos, Lima",
                "+51 1 555 2003",
                4.9m,
                ["DIESEL", "GASOHOL_90", "GNV", "GLP"],
                "Soluciones integrales de abastecimiento continuo 24/7."));
            context.Set<ProviderCompany>().Add(provider3);
            context.SaveChanges();
        }

        // 4. Users (published environments supply a private demo password).
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var userBuyer = new User(new SignUpCommand(
            "Transportes del Sur S.A.",
            "logistics@transportesdelsur.com",
            "logistics@transportesdelsur.com",
            password,
            "BUYER",
            buyer1.Id), passwordHash);
        context.Set<User>().Add(userBuyer);

        var userProvider = new User(new SignUpCommand(
            "Petro Andes Distribuidora S.A.C.",
            "dispatch@petroandes.com",
            "dispatch@petroandes.com",
            password,
            "PROVIDER",
            provider1.Id), passwordHash);
        context.Set<User>().Add(userProvider);
        context.SaveChanges();

        // 5. Catalog Products
        if (!context.Set<ProviderProduct>().Any(p => p.ProviderId == provider1.Id))
        {
            context.Set<ProviderProduct>().AddRange(
                new ProviderProduct(new CreateProviderProductCommand(provider1.Id, "DIESEL", "Diesel B5 S-50", "Diesel bajo en azufre de alta eficiencia", 4.50m, "L", true)),
                new ProviderProduct(new CreateProviderProductCommand(provider1.Id, "GLP", "GLP Automotor e Industrial", "Gas Licuado de Petróleo de alta pureza", 2.80m, "L", true)),
                new ProviderProduct(new CreateProviderProductCommand(provider1.Id, "GNV", "GNV Comprimido", "Gas Natural Vehicular comprimido", 1.95m, "M3", true)),
                new ProviderProduct(new CreateProviderProductCommand(provider1.Id, "GASOHOL_95", "Gasohol Premium 95 Plus", "Gasohol 95 octanos aditivado", 5.20m, "L", true)),
                new ProviderProduct(new CreateProviderProductCommand(provider1.Id, "GASOHOL_90", "Gasohol Regular 90", "Gasohol 90 octanos para flotas", 4.80m, "L", true)),
                new ProviderProduct(new CreateProviderProductCommand(provider2.Id, "DIESEL", "Diesel B5 Marino", "Combustible diesel grado marino y pesado", 4.45m, "L", true)),
                new ProviderProduct(new CreateProviderProductCommand(provider2.Id, "GLP", "GLP Industrial", "Suministro continuo de GLP", 2.75m, "L", true)),
                new ProviderProduct(new CreateProviderProductCommand(provider3.Id, "DIESEL", "Diesel Ultra Limpio B5", "Diesel ultra filtrado para maquinaria", 4.55m, "L", true)),
                new ProviderProduct(new CreateProviderProductCommand(provider3.Id, "GLP", "GLP Granel", "GLP para montacargas y calderas", 2.85m, "L", true))
            );
            context.SaveChanges();
        }

        // 6. Inventory Items (Required by OrderCommandService to calculate authoritative price and reserve stock)
        if (!context.Set<InventoryItem>().Any(i => i.ProviderId == provider1.Id))
        {
            context.Set<InventoryItem>().AddRange(
                new InventoryItem(new CreateInventoryItemCommand(
                    provider1.Id, "Tanque Diesel Principal T-01", "DIESEL", "Tanque principal de almacenamiento Diesel B5",
                    4.50m, 75000, 0, 100000, 15000, "L", "ACTIVE")),
                new InventoryItem(new CreateInventoryItemCommand(
                    provider1.Id, "Tanque Esférico GLP-01", "GLP", "Esfera de almacenamiento de GLP presurizado",
                    2.80m, 40000, 0, 60000, 10000, "L", "ACTIVE")),
                new InventoryItem(new CreateInventoryItemCommand(
                    provider1.Id, "Compresor de GNV C-01", "GNV", "Estación de compresión y despacho GNV",
                    1.95m, 25000, 0, 35000, 5000, "M3", "ACTIVE")),
                new InventoryItem(new CreateInventoryItemCommand(
                    provider1.Id, "Tanque Gasohol 95 T-02", "GASOHOL_95", "Tanque subterráneo para Gasohol 95",
                    5.20m, 50000, 0, 80000, 12000, "L", "ACTIVE")),
                new InventoryItem(new CreateInventoryItemCommand(
                    provider1.Id, "Tanque Gasohol 90 T-03", "GASOHOL_90", "Tanque subterráneo para Gasohol 90",
                    4.80m, 45000, 0, 70000, 10000, "L", "ACTIVE"))
            );
            context.SaveChanges();
        }

        // 7. Fleet (Vehicles & Drivers for Fulfillment)
        Vehicle? v1 = null;
        Vehicle? v2 = null;
        if (!context.Set<Vehicle>().Any(v => v.ProviderId == provider1.Id))
        {
            v1 = new Vehicle(new CreateVehicleCommand("ABC-123", "Volvo", "FMX 440 Cisterna 15KL", 15000, "L", "AVAILABLE", provider1.Id));
            v2 = new Vehicle(new CreateVehicleCommand("GHI-789", "Scania", "P380 GLP Cisterna 12KL", 12000, "L", "AVAILABLE", provider1.Id));
            var v3 = new Vehicle(new CreateVehicleCommand("JKL-012", "Hino", "500 Cisterna Urbana 8KL", 8000, "L", "AVAILABLE", provider1.Id));
            context.Set<Vehicle>().AddRange(v1, v2, v3);
            context.SaveChanges();
        }
        else
        {
            v1 = context.Set<Vehicle>().FirstOrDefault(v => v.ProviderId == provider1.Id);
            v2 = context.Set<Vehicle>().Skip(1).FirstOrDefault(v => v.ProviderId == provider1.Id);
        }

        Driver? d1 = null;
        Driver? d2 = null;
        if (!context.Set<Driver>().Any(d => d.ProviderId == provider1.Id))
        {
            d1 = new Driver(new CreateDriverCommand("Carlos Mendoza", "Q12345678", "+51 987 654 321", "carlos.mendoza@petroandes.pe", "AVAILABLE", provider1.Id));
            d2 = new Driver(new CreateDriverCommand("Jorge Silva", "Q23456789", "+51 987 654 322", "jorge.silva@petroandes.pe", "AVAILABLE", provider1.Id));
            var d3 = new Driver(new CreateDriverCommand("Raúl Paredes", "Q34567890", "+51 987 654 323", "raul.paredes@petroandes.pe", "AVAILABLE", provider1.Id));
            context.Set<Driver>().AddRange(d1, d2, d3);
            context.SaveChanges();
        }
        else
        {
            d1 = context.Set<Driver>().FirstOrDefault(d => d.ProviderId == provider1.Id);
            d2 = context.Set<Driver>().Skip(1).FirstOrDefault(d => d.ProviderId == provider1.Id);
        }

        // 8. Buyer Equipment (Linked to buyer1)
        EquipmentAggregate? eqForklift = null;
        EquipmentAggregate? eqGenerator = null;
        EquipmentAggregate? eqExcavator = null;
        EquipmentAggregate? eqTank = null;

        if (!context.Set<EquipmentAggregate>().Any(e => e.CompanyId == buyer1.Id))
        {
            eqForklift = new EquipmentAggregate(new CreateEquipmentCommand(
                buyer1.Id, "Forklift Fleet FL-3", "FORKLIFT", "GLP", 80, 60, "L", "operational",
                provider1.Id, false, 20, "Warehouse Callao", DateTimeOffset.UtcNow.AddDays(-2).ToString("yyyy-MM-dd HH:mm")));

            eqGenerator = new EquipmentAggregate(new CreateEquipmentCommand(
                buyer1.Id, "Main Generator G-1", "GENERATOR", "DIESEL", 500, 120, "L", "low_fuel",
                provider1.Id, true, 150, "Plant Norte, Trujillo", DateTimeOffset.UtcNow.AddDays(-1).ToString("yyyy-MM-dd HH:mm")));

            eqExcavator = new EquipmentAggregate(new CreateEquipmentCommand(
                buyer1.Id, "Excavator CAT 320", "EXCAVATOR", "DIESEL", 400, 40, "L", "critical",
                provider1.Id, false, 80, "Mining Site Arequipa", DateTimeOffset.UtcNow.AddDays(-5).ToString("yyyy-MM-dd HH:mm")));

            eqTank = new EquipmentAggregate(new CreateEquipmentCommand(
                buyer1.Id, "Tank Stationary T-100", "TANK", "DIESEL", 5000, 3200, "L", "operational",
                provider1.Id, true, 1000, "Central Depot Lima", DateTimeOffset.UtcNow.AddDays(-3).ToString("yyyy-MM-dd HH:mm")));

            context.Set<EquipmentAggregate>().AddRange(eqForklift, eqGenerator, eqExcavator, eqTank);
            context.SaveChanges();
        }
        else
        {
            eqForklift = context.Set<EquipmentAggregate>().FirstOrDefault(e => e.CompanyId == buyer1.Id && e.RequiredFuelType == "GLP");
            eqGenerator = context.Set<EquipmentAggregate>().FirstOrDefault(e => e.CompanyId == buyer1.Id && e.RequiredFuelType == "DIESEL");
            eqExcavator = context.Set<EquipmentAggregate>().Skip(2).FirstOrDefault(e => e.CompanyId == buyer1.Id);
            eqTank = context.Set<EquipmentAggregate>().Skip(3).FirstOrDefault(e => e.CompanyId == buyer1.Id);
        }

        // 9. Initial Fuel Requests and Orders for end-to-end testing
        if (!context.Set<Request>().Any(r => r.BuyerCompanyId == buyer1.Id))
        {
            // Pending Request 1 (For Forklift FL-3, GLP)
            var req1 = new Request(new CreateRequestCommand(
                buyer1.Id, provider1.Id, eqForklift?.Id, "GLP", "GLP Automotor e Industrial",
                20, "L", 2.80m, "Warehouse Callao", DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
                "PENDING", "MANUAL", null, null));

            // Pending Request 2 (For Main Generator G-1, DIESEL)
            var req2 = new Request(new CreateRequestCommand(
                buyer1.Id, provider1.Id, eqGenerator?.Id, "DIESEL", "Diesel B5 S-50",
                300, "L", 4.50m, "Plant Norte, Trujillo", DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd"),
                "PENDING", "AUTO", null, null));

            context.Set<Request>().AddRange(req1, req2);
            context.SaveChanges();

            // Order 1: Status ACCEPTED (Created from an approved request, ready for provider dispatch)
            var orderAccepted = new Order(new CreateOrderCommand(
                null, buyer1.Id, provider1.Id, eqForklift?.Id, "GLP", 20, "L", 2.80m, 56.00m,
                "Warehouse Callao", "ACCEPTED", "PENDING", null, null,
                DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"), null, null, null, null, null, null));

            // Order 2: Status DISPATCHED (Dispatched with fleet, ready for buyer reception)
            var orderDispatched = new Order(new CreateOrderCommand(
                null, buyer1.Id, provider1.Id, eqGenerator?.Id, "DIESEL", 150, "L", 4.50m, 675.00m,
                "Plant Norte, Trujillo", "DISPATCHED", "PENDING", d1?.Id, v1?.Id,
                DateTime.UtcNow.ToString("yyyy-MM-dd"), DateTime.UtcNow.AddHours(-1).ToString("O"),
                null, null, null, null, null));

            // Order 3: Status PENDING_PAYMENT (Delivered, ready for buyer payment)
            var orderDelivered = new Order(new CreateOrderCommand(
                null, buyer1.Id, provider1.Id, eqTank?.Id, "DIESEL", 460, "L", 4.50m, 2070.00m,
                "Central Depot Lima", "PENDING_PAYMENT", "PENDING", d2?.Id, v2?.Id,
                DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"),
                DateTime.UtcNow.AddDays(-1).AddHours(2).ToString("O"),
                DateTime.UtcNow.AddDays(-1).AddHours(4).ToString("O"),
                null, null, null, null));

            context.Set<Order>().AddRange(orderAccepted, orderDispatched, orderDelivered);
            context.SaveChanges();

            // Also create delivery for the dispatched order so fulfillment queries find it
            if (v1 is not null && d1 is not null)
            {
                var delivery = new Delivery(new CreateDeliveryCommand(
                    orderDispatched.Id, provider1.Id, d1.Id, v1.Id, "on_the_way",
                    "Carretera Panamericana Sur Km 18, Villa El Salvador, Lima",
                    "Plant Norte, Trujillo", DateTime.UtcNow.ToString("O"), null, "Despacho prioritario"));
                context.Set<Delivery>().Add(delivery);
                context.SaveChanges();
            }
        }

        // 10. Notifications
        if (!context.Set<NotificationEntity>().Any(n => n.BuyerCompanyId == buyer1.Id || n.ProviderId == provider1.Id))
        {
            context.Set<NotificationEntity>().AddRange(
                new NotificationEntity(new CreateNotificationCommand(
                    "BUYER", buyer1.Id, provider1.Id, "SYSTEM", "Bienvenido a FullTank",
                    "Tu cuenta de comprador ha sido configurada correctamente.", false, null, "/dashboard")),
                new NotificationEntity(new CreateNotificationCommand(
                    "PROVIDER", buyer1.Id, provider1.Id, "SYSTEM", "Bienvenido a FullTank",
                    "Tu cuenta de distribuidor ha sido configurada correctamente.", false, null, "/dashboard"))
            );
            context.SaveChanges();
        }
        transaction.Commit();
    }
}
