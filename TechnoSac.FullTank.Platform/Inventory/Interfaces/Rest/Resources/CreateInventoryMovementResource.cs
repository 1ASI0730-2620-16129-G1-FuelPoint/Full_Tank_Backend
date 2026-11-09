namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;

public record CreateInventoryMovementResource(
    int? InventoryItemId,
    int? ProviderId,
    string Type,
    int Quantity,
    string Reason,
    int? OrderId);
