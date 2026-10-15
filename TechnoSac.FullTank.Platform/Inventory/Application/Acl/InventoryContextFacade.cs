using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Domain.Services;

namespace TechnoSac.FullTank.Platform.Inventory.Application.Acl;

public class InventoryContextFacade(
    IInventoryItemRepository itemRepository,
    IInventoryMovementRepository movementRepository,
    IUnitOfWork unitOfWork) : IInventoryContextFacade
{
    public async Task<decimal?> FetchPricePerLiterAsync(int providerId, string fuelType, CancellationToken cancellationToken)
    {
        var item = await itemRepository.FindByProviderIdAndTypeAsync(providerId, fuelType, cancellationToken);
        if (item is null) return null;
        if (!string.Equals(item.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(item.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase))
            return null;
        return item.PricePerLiter;
    }

    public async Task<bool> ReserveStockAsync(int providerId, string fuelType, int quantity, string unit = "LITERS",
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return false;
        var item = await itemRepository.FindByProviderIdAndTypeAsync(providerId, fuelType, cancellationToken);
        if (item is null) return false;

        // Facades validate ACTIVE tank
        if (!string.Equals(item.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(item.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase))
            return false;

        int stockUnits;
        try
        {
            stockUnits = FuelUnitConverter.ConvertToStockUnitsCeil(quantity, unit, item.Unit);
        }
        catch (OverflowException)
        {
            return false;
        }

        if (stockUnits <= 0) return false;

        try
        {
            item.Reserve(stockUnits);
            itemRepository.Update(item);
            await unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ReleaseStockAsync(int providerId, string fuelType, int quantity, string unit = "LITERS",
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return false;
        var item = await itemRepository.FindByProviderIdAndTypeAsync(providerId, fuelType, cancellationToken);
        if (item is null) return false;

        int stockUnits;
        try
        {
            stockUnits = FuelUnitConverter.ConvertToStockUnitsCeil(quantity, unit, item.Unit);
        }
        catch (OverflowException)
        {
            return false;
        }

        if (stockUnits <= 0) return false;

        try
        {
            item.ReleaseReservation(stockUnits);
            itemRepository.Update(item);
            await unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> FulfillStockAsync(int providerId, string fuelType, int quantity, string unit, int orderId,
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) return false;
        var item = await itemRepository.FindByProviderIdAndTypeAsync(providerId, fuelType, cancellationToken);
        if (item is null) return false;

        // Facades validate ACTIVE tank
        if (!string.Equals(item.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(item.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase))
            return false;

        int stockUnits;
        try
        {
            stockUnits = FuelUnitConverter.ConvertToStockUnitsCeil(quantity, unit, item.Unit);
        }
        catch (OverflowException)
        {
            return false;
        }

        if (stockUnits <= 0) return false;

        try
        {
            item.FulfillReservation(stockUnits);
            itemRepository.Update(item);

            var movement = new InventoryMovement(new CreateInventoryMovementCommand(
                item.Id,
                providerId,
                "OUT",
                stockUnits,
                $"Order #{orderId} dispatch ({quantity} {unit})",
                orderId));
            await movementRepository.AddAsync(movement, cancellationToken);

            await unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Task<bool> FulfillStockAsync(int providerId, string fuelType, int quantity, int orderId,
        CancellationToken cancellationToken = default)
    {
        return FulfillStockAsync(providerId, fuelType, quantity, "LITERS", orderId, cancellationToken);
    }
}
