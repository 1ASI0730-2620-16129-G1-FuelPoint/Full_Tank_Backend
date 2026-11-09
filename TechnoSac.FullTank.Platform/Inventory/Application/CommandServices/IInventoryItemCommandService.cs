using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Application.Model;

namespace TechnoSac.FullTank.Platform.Inventory.Application.CommandServices;

public interface IInventoryItemCommandService
{
    Task<Result<InventoryItem>> Handle(CreateInventoryItemCommand command, CancellationToken cancellationToken);
    Task<Result<InventoryItem>> Handle(UpdateInventoryItemCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteInventoryItemCommand command, CancellationToken cancellationToken);
}
