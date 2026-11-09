using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;

public interface IInventoryItemRepository : IBaseRepository<InventoryItem>
{
    Task<IEnumerable<InventoryItem>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken);
    Task<InventoryItem?> FindByProviderIdAndTypeAsync(int providerId, string type, CancellationToken cancellationToken);
}
