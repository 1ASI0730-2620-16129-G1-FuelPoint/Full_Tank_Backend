namespace TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;

public record UpdateInventoryItemCommand(
    int Id,
    int? ProviderId,
    string Name,
    string Type,
    string Description,
    decimal PricePerLiter,
    int Stock,
    int Reserved,
    int Capacity,
    int LowStockThreshold,
    string Unit,
    string Status);
