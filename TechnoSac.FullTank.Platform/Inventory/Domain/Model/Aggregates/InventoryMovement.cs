using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a stock movement (IN/OUT) for an inventory item.</summary>
public class InventoryMovement : IAuditableEntity
{
    protected InventoryMovement()
    {
        Type = string.Empty;
        Reason = string.Empty;
    }

    public InventoryMovement(CreateInventoryMovementCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        InventoryItemId = command.InventoryItemId;
        ProviderId = command.ProviderId;
        Type = command.Type;
        Quantity = command.Quantity;
        Reason = command.Reason;
        OrderId = command.OrderId;
    }

    public int Id { get; private set; }
    public int? InventoryItemId { get; private set; }
    public int? ProviderId { get; private set; }
    public string Type { get; private set; }
    public int Quantity { get; private set; }
    public string Reason { get; private set; }
    public int? OrderId { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
