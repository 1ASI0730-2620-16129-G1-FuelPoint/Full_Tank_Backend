using TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;

namespace TechnoSac.FullTank.Platform.Equipment.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class EquipmentRepository(AppDbContext context)
    : BaseRepository<EquipmentAggregate>(context), IEquipmentRepository
{
    public async Task<IEnumerable<EquipmentAggregate>> FindByCompanyIdAsync(int companyId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<EquipmentAggregate>()
            .Where(equipment => equipment.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }
}
