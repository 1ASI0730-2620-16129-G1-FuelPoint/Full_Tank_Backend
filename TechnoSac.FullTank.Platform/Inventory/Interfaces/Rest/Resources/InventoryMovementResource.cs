namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;

public record InventoryMovementResource(
    int Id,
    int? InventoryItemId,
    int? ProviderId,
    string Type,
    int Quantity,
    string Reason,
    int? OrderId,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
