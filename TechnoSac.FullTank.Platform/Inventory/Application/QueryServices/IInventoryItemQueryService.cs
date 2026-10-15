using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Inventory.Application.QueryServices;

public interface IInventoryItemQueryService
{
    Task<IEnumerable<InventoryItem>> Handle(GetAllInventoryItemsQuery query, CancellationToken cancellationToken);
    Task<InventoryItem?> Handle(GetInventoryItemByIdQuery query, CancellationToken cancellationToken);
}
