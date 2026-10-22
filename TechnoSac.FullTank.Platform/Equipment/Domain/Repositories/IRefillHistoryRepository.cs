using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;

public interface IRefillHistoryRepository : IBaseRepository<RefillHistory>
{
    Task<IEnumerable<RefillHistory>> FindByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken);
}
