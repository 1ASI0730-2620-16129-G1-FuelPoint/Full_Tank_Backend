using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Inventory.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class InventoryMovementRepository(AppDbContext context)
    : BaseRepository<InventoryMovement>(context), IInventoryMovementRepository
{
    public async Task<IEnumerable<InventoryMovement>> FindByInventoryItemIdAsync(int inventoryItemId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<InventoryMovement>()
            .Where(movement => movement.InventoryItemId == inventoryItemId)
            .ToListAsync(cancellationToken);
    }
}
