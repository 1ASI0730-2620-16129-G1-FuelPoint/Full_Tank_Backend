using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Transform;

public static class UpdateInventoryItemCommandFromResourceAssembler
{
    public static UpdateInventoryItemCommand ToCommandFromResource(int id, UpdateInventoryItemResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateInventoryItemCommand(id, resource.ProviderId, resource.Name, resource.Type,
            resource.Description, resource.PricePerLiter, resource.Stock, resource.Reserved, resource.Capacity,
            resource.LowStockThreshold, resource.Unit, resource.Status);
    }
}
