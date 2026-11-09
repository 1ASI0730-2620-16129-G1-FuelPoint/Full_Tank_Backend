using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Equipment.Application.QueryServices;

public interface IRefillHistoryQueryService
{
    Task<IEnumerable<RefillHistory>> Handle(GetAllRefillHistoryQuery query, CancellationToken cancellationToken);
    Task<RefillHistory?> Handle(GetRefillHistoryByIdQuery query, CancellationToken cancellationToken);
}
