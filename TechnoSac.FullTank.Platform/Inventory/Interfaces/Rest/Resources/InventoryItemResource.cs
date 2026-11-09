namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;

public record InventoryItemResource(
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
    string Status,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
