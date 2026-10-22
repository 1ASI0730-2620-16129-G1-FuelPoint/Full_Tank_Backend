using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;

namespace TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Transform;

public static class InventoryItemResourceFromEntityAssembler
{
    public static InventoryItemResource ToResourceFromEntity(InventoryItem entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new InventoryItemResource(entity.Id, entity.ProviderId, entity.Name, entity.Type, entity.Description,
            entity.PricePerLiter, entity.Stock, entity.Reserved, entity.Capacity, entity.LowStockThreshold, entity.Unit,
            entity.Status, entity.CreatedAt, entity.UpdatedAt);
    }
}
