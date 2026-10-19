using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Equipment.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class RefillHistoryRepository(AppDbContext context)
    : BaseRepository<RefillHistory>(context), IRefillHistoryRepository
{
    public async Task<IEnumerable<RefillHistory>> FindByEquipmentIdAsync(int equipmentId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<RefillHistory>()
            .Where(refill => refill.EquipmentId == equipmentId)
            .ToListAsync(cancellationToken);
    }
}
