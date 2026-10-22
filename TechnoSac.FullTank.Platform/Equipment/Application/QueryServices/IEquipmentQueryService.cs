using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Queries;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;

namespace TechnoSac.FullTank.Platform.Equipment.Application.QueryServices;

public interface IEquipmentQueryService
{
    Task<IEnumerable<EquipmentAggregate>> Handle(GetAllEquipmentQuery query, CancellationToken cancellationToken);
    Task<EquipmentAggregate?> Handle(GetEquipmentByIdQuery query, CancellationToken cancellationToken);
}
