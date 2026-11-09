using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Services;
using Xunit;

namespace TechnoSac.FullTank.Platform.Tests;

public class DomainInvariantsTests
{
    [Fact]
    public void Equipment_InvalidCapacityOrLevels_ThrowsArgumentException()
    {
        // Zero or negative capacity
        Assert.Throws<ArgumentException>(() => new EquipmentAggregate(
            new CreateEquipmentCommand(
                CompanyId: 1,
                Name: "Generator A",
                Type: "GENERATOR",
                RequiredFuelType: "DIESEL",
                Capacity: 0,
                CurrentLevel: 0,
                Unit: "L",
                Status: "operational",
                FavoriteProviderId: null,
                AutoRefill: false,
                RefillThreshold: 0,
                Location: "Site 1",
                LastRefillDate: null)));

        // CurrentLevel > Capacity
        Assert.Throws<ArgumentException>(() => new EquipmentAggregate(
            new CreateEquipmentCommand(
                CompanyId: 1,
                Name: "Generator A",
                Type: "GENERATOR",
                RequiredFuelType: "DIESEL",
                Capacity: 100,
                CurrentLevel: 150,
                Unit: "L",
                Status: "operational",
                FavoriteProviderId: null,
                AutoRefill: false,
                RefillThreshold: 20,
                Location: "Site 1",
                LastRefillDate: null)));

        // RefillThreshold > Capacity
        Assert.Throws<ArgumentException>(() => new EquipmentAggregate(
            new CreateEquipmentCommand(
                CompanyId: 1,
                Name: "Generator A",
                Type: "GENERATOR",
                RequiredFuelType: "DIESEL",
                Capacity: 100,
                CurrentLevel: 50,
                Unit: "L",
                Status: "operational",
                FavoriteProviderId: null,
                AutoRefill: false,
                RefillThreshold: 120,
                Location: "Site 1",
                LastRefillDate: null)));
    }

    [Fact]
    public void Equipment_Refill_IncreasesLevelUpToCapacity()
    {
        var equipment = new EquipmentAggregate(
            new CreateEquipmentCommand(
                CompanyId: 1,
                Name: "Generator A",
                Type: "GENERATOR",
                RequiredFuelType: "DIESEL",
                Capacity: 500,
                CurrentLevel: 100,
                Unit: "L",
                Status: "operational",
                FavoriteProviderId: null,
                AutoRefill: false,
                RefillThreshold: 50,
                Location: "Site 1",
                LastRefillDate: null));

        equipment.Refill(200);
        Assert.Equal(300, equipment.CurrentLevel);

        // Refill exceeding capacity clamps to capacity
        equipment.Refill(300);
        Assert.Equal(500, equipment.CurrentLevel);
    }

    [Fact]
    public void Equipment_DeriveStatus_TransitionsOperationalLowFuelCritical()
    {
        // Start near empty: critical status
        var equipment = new EquipmentAggregate(
            new CreateEquipmentCommand(
                CompanyId: 1,
                Name: "Truck A",
                Type: "TRUCK",
                RequiredFuelType: "DIESEL",
                Capacity: 500,
                CurrentLevel: 20,
                Unit: "L",
                Status: "critical",
                FavoriteProviderId: null,
                AutoRefill: false,
                RefillThreshold: 80,
                Location: "Site 1",
                LastRefillDate: null));

        Assert.Equal("critical", equipment.Status);

        // Refill to low_fuel zone (CurrentLevel 60 <= RefillThreshold 80)
        equipment.Refill(40);
        Assert.Equal(60, equipment.CurrentLevel);
        Assert.Equal("low_fuel", equipment.Status);

        // Refill to operational zone (CurrentLevel 200 > RefillThreshold 80)
        equipment.Refill(140);
        Assert.Equal(200, equipment.CurrentLevel);
        Assert.Equal("operational", equipment.Status);
    }

    [Fact]
    public void InventoryItem_Invariants_EnforcesCapacityAndStockRules()
    {
        // Negative stock throws
        Assert.Throws<ArgumentException>(() => new InventoryItem(
            new CreateInventoryItemCommand(
                ProviderId: 1,
                Name: "Diesel Tank",
                Type: "DIESEL",
                Description: "Primary tank",
                PricePerLiter: 4.5m,
                Stock: -10,
                Reserved: 0,
                Capacity: 1000,
                LowStockThreshold: 100,
                Unit: "L",
                Status: "AVAILABLE")));

        // Stock exceeds capacity throws
        Assert.Throws<ArgumentException>(() => new InventoryItem(
            new CreateInventoryItemCommand(
                ProviderId: 1,
                Name: "Diesel Tank",
                Type: "DIESEL",
                Description: "Primary tank",
                PricePerLiter: 4.5m,
                Stock: 1200,
                Reserved: 0,
                Capacity: 1000,
                LowStockThreshold: 100,
                Unit: "L",
                Status: "AVAILABLE")));

        // Reserved exceeds stock throws
        Assert.Throws<ArgumentException>(() => new InventoryItem(
            new CreateInventoryItemCommand(
                ProviderId: 1,
                Name: "Diesel Tank",
                Type: "DIESEL",
                Description: "Primary tank",
                PricePerLiter: 4.5m,
                Stock: 100,
                Reserved: 200,
                Capacity: 1000,
                LowStockThreshold: 100,
                Unit: "L",
                Status: "AVAILABLE")));
    }

    [Fact]
    public void InventoryItem_Reserve_And_Fulfill_ManagesStockCorrectly()
    {
        var item = new InventoryItem(
            new CreateInventoryItemCommand(
                ProviderId: 1,
                Name: "Diesel Tank",
                Type: "DIESEL",
                Description: "Primary tank",
                PricePerLiter: 4.5m,
                Stock: 500,
                Reserved: 0,
                Capacity: 1000,
                LowStockThreshold: 100,
                Unit: "L",
                Status: "AVAILABLE"));

        item.Reserve(200);
        Assert.Equal(500, item.Stock);
        Assert.Equal(200, item.Reserved);

        // Over-reservation throws InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => item.Reserve(400));

        // Fulfill reservation reduces stock and reserved
        item.FulfillReservation(200);
        Assert.Equal(300, item.Stock);
        Assert.Equal(0, item.Reserved);
    }

    [Fact]
    public void Request_Approve_OnlyValidWhenPending()
    {
        var command = new CreateRequestCommand(
            BuyerCompanyId: 1,
            ProviderId: 2,
            EquipmentId: 3,
            FuelType: "DIESEL",
            ProductName: "Diesel B5",
            Quantity: 100,
            Unit: "L",
            UnitPrice: 4.5m,
            DeliveryAddress: "Address",
            DeliveryDate: "2026-09-10",
            Status: "PENDING",
            Source: "MANUAL",
            RejectionReasonCode: null,
            RejectionReasonNote: null);
        var request = new Request(command);

        request.Approve();
        Assert.Equal("APPROVED", request.Status);

        // Cannot approve again
        Assert.Throws<InvalidOperationException>(() => request.Approve());
    }

    [Fact]
    public void Order_WorkflowStateTransitions_EnforceOrderLifecycle()
    {
        var command = new CreateOrderCommand(
            RequestId: 1,
            BuyerCompanyId: 1,
            ProviderId: 2,
            EquipmentId: 3,
            FuelType: "DIESEL",
            Quantity: 100,
            Unit: "L",
            UnitPrice: 4.5m,
            TotalAmount: 450m,
            DeliveryAddress: "Address",
            Status: "ACCEPTED",
            PaymentStatus: "PENDING",
            DriverId: null,
            VehicleId: null,
            EstimatedDeliveryDate: "2026-09-10",
            DispatchedAt: null,
            DeliveredAt: null,
            PaidAt: null,
            ClosedAt: null,
            CancelledAt: null,
            CancelReason: null);
        var order = new Order(command);

        // Dispatch requires ACCEPTED status
        order.Dispatch(10, 20);
        Assert.Equal("DISPATCHED", order.Status);
        Assert.Equal(10, order.DriverId);
        Assert.Equal(20, order.VehicleId);

        // Cannot dispatch again
        Assert.Throws<InvalidOperationException>(() => order.Dispatch(10, 20));

        // Receive transitions to PENDING_PAYMENT
        order.Receive();
        Assert.Equal("PENDING_PAYMENT", order.Status);

        // MarkPaid transitions to PAID
        order.MarkPaid();
        Assert.Equal("PAID", order.Status);
        Assert.Equal("PAID", order.PaymentStatus);

        // Cannot cancel a paid order
        Assert.Throws<InvalidOperationException>(() => order.Cancel("Refund"));
    }

    [Fact]
    public void FuelUnitConverter_UnitRecognition_And_RoundingRules()
    {
        // Recognition of short and long unit names
        Assert.True(FuelUnitConverter.IsGallon("GAL"));
        Assert.True(FuelUnitConverter.IsGallon("GL"));
        Assert.True(FuelUnitConverter.IsGallon("GALLON"));
        Assert.True(FuelUnitConverter.IsGallon("GALLONS"));
        Assert.True(FuelUnitConverter.IsGallon("  gallons  "));

        Assert.True(FuelUnitConverter.IsLiter("L"));
        Assert.True(FuelUnitConverter.IsLiter("LT"));
        Assert.True(FuelUnitConverter.IsLiter("LITER"));
        Assert.True(FuelUnitConverter.IsLiter("LITERS"));
        Assert.True(FuelUnitConverter.IsLiter("  liters  "));

        // Conservative ceiling rule for inventory stock reservation:
        // 100 gallons * 3.78541 = 378.541 -> ceiling = 379 liters
        Assert.Equal(379, FuelUnitConverter.ConvertToStockUnitsCeil(100, "GALLONS", "LITERS"));
        // 1 gallon * 3.78541 = 3.78541 -> ceiling = 4 liters
        Assert.Equal(4, FuelUnitConverter.ConvertToStockUnitsCeil(1, "GALLONS", "LITERS"));
        // Same unit requires no conversion
        Assert.Equal(100, FuelUnitConverter.ConvertToStockUnitsCeil(100, "LITERS", "LITERS"));
        Assert.Equal(50, FuelUnitConverter.ConvertToStockUnitsCeil(50, "GALLONS", "GALLONS"));

        // Banker's rounding for equipment refills
        Assert.Equal(379, FuelUnitConverter.ConvertToEquipmentUnits(100, "GALLONS", "LITERS"));

        // Authoritative unit price calculation (per liter to per gallon)
        // 3.50 * 3.78541 = 13.248935 -> rounded to 13.25
        Assert.Equal(13.25m, FuelUnitConverter.CalculateUnitPrice(3.50m, "GALLONS"));
        Assert.Equal(3.50m, FuelUnitConverter.CalculateUnitPrice(3.50m, "LITERS"));

        // Overflow protection
        Assert.Throws<OverflowException>(() =>
            FuelUnitConverter.ConvertToStockUnitsCeil(decimal.MaxValue, "GALLONS", "LITERS"));
    }
}
