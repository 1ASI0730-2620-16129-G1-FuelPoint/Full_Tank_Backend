using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Inventory.Application.CommandServices;

public interface IInventoryMovementCommandService
{
    Task<Result<InventoryMovement>> Handle(CreateInventoryMovementCommand command, CancellationToken cancellationToken);
}
