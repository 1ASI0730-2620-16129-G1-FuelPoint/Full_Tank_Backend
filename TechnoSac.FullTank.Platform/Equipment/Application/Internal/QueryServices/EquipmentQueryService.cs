using TechnoSac.FullTank.Platform.Equipment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;

namespace TechnoSac.FullTank.Platform.Equipment.Application.Internal.QueryServices;

public class EquipmentQueryService(IEquipmentRepository repository) : IEquipmentQueryService
{
    public async Task<IEnumerable<EquipmentAggregate>> Handle(GetAllEquipmentQuery query,
        CancellationToken cancellationToken)
    {
        return query.CompanyId is null
            ? await repository.ListAsync(cancellationToken)
            : await repository.FindByCompanyIdAsync(query.CompanyId.Value, cancellationToken);
    }

    public async Task<EquipmentAggregate?> Handle(GetEquipmentByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
