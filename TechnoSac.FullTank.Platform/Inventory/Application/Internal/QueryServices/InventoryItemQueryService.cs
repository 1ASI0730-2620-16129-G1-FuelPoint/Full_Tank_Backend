using TechnoSac.FullTank.Platform.Inventory.Application.QueryServices;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Inventory.Application.Internal.QueryServices;

public class InventoryItemQueryService(IInventoryItemRepository repository) : IInventoryItemQueryService
{
    public async Task<IEnumerable<InventoryItem>> Handle(GetAllInventoryItemsQuery query,
        CancellationToken cancellationToken)
    {
        return query.ProviderId is null
            ? await repository.ListAsync(cancellationToken)
            : await repository.FindByProviderIdAsync(query.ProviderId.Value, cancellationToken);
    }

    public async Task<InventoryItem?> Handle(GetInventoryItemByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
