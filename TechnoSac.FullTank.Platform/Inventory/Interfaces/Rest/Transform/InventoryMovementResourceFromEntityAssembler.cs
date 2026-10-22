using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Transform;

public static class InventoryMovementResourceFromEntityAssembler
{
    public static InventoryMovementResource ToResourceFromEntity(InventoryMovement entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new InventoryMovementResource(entity.Id, entity.InventoryItemId, entity.ProviderId, entity.Type,
            entity.Quantity, entity.Reason, entity.OrderId, entity.CreatedAt, entity.UpdatedAt);
    }
}
