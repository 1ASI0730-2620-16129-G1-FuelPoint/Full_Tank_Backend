using TechnoSac.FullTank.Platform.Equipment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Equipment.Application.Internal.QueryServices;

public class RefillHistoryQueryService(IRefillHistoryRepository repository) : IRefillHistoryQueryService
{
    public async Task<IEnumerable<RefillHistory>> Handle(GetAllRefillHistoryQuery query,
        CancellationToken cancellationToken)
    {
        return query.EquipmentId is null
            ? await repository.ListAsync(cancellationToken)
            : await repository.FindByEquipmentIdAsync(query.EquipmentId.Value, cancellationToken);
    }

    public async Task<RefillHistory?> Handle(GetRefillHistoryByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
