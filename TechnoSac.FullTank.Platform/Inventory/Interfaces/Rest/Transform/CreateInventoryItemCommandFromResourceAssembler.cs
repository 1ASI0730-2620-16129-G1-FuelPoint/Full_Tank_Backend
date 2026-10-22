using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Transform;

public static class CreateInventoryItemCommandFromResourceAssembler
{
    public static CreateInventoryItemCommand ToCommandFromResource(CreateInventoryItemResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new CreateInventoryItemCommand(resource.ProviderId, resource.Name, resource.Type, resource.Description,
            resource.PricePerLiter, resource.Stock, resource.Reserved, resource.Capacity, resource.LowStockThreshold,
            resource.Unit, resource.Status);
    }
}
