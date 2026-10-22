using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Inventory.Application.QueryServices;

public interface IInventoryMovementQueryService
{
    Task<IEnumerable<InventoryMovement>> Handle(GetAllInventoryMovementsQuery query, CancellationToken cancellationToken);
    Task<InventoryMovement?> Handle(GetInventoryMovementByIdQuery query, CancellationToken cancellationToken);
}
