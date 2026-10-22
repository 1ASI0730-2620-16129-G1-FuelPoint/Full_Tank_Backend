using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;

public interface IInventoryMovementRepository : IBaseRepository<InventoryMovement>
{
    Task<IEnumerable<InventoryMovement>> FindByInventoryItemIdAsync(int inventoryItemId,
        CancellationToken cancellationToken);
}
