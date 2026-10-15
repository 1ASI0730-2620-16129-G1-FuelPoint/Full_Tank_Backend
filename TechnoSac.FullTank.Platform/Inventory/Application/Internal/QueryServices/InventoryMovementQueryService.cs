using TechnoSac.FullTank.Platform.Inventory.Application.QueryServices;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Inventory.Application.Internal.QueryServices;

public class InventoryMovementQueryService(IInventoryMovementRepository repository) : IInventoryMovementQueryService
{
    public async Task<IEnumerable<InventoryMovement>> Handle(GetAllInventoryMovementsQuery query,
        CancellationToken cancellationToken)
    {
        return query.InventoryItemId is null
            ? await repository.ListAsync(cancellationToken)
            : await repository.FindByInventoryItemIdAsync(query.InventoryItemId.Value, cancellationToken);
    }

    public async Task<InventoryMovement?> Handle(GetInventoryMovementByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
