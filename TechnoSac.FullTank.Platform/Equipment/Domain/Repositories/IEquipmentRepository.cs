using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;

public interface IEquipmentRepository : IBaseRepository<EquipmentAggregate>
{
    Task<IEnumerable<EquipmentAggregate>> FindByCompanyIdAsync(int companyId, CancellationToken cancellationToken);
}
