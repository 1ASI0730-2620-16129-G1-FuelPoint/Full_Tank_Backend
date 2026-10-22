namespace TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;

public record CreateInventoryMovementCommand(
    int? InventoryItemId,
    int? ProviderId,
    string Type,
    int Quantity,
    string Reason,
    int? OrderId);
