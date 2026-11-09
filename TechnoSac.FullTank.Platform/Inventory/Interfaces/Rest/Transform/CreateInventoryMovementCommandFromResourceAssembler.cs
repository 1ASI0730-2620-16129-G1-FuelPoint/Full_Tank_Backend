using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Transform;

public static class CreateInventoryMovementCommandFromResourceAssembler
{
    public static CreateInventoryMovementCommand ToCommandFromResource(CreateInventoryMovementResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateInventoryMovementCommand(resource.InventoryItemId, resource.ProviderId, resource.Type,
            resource.Quantity, resource.Reason, resource.OrderId);
    }
}
