using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;

public interface IDriverQueryService
{
    Task<IEnumerable<Driver>> Handle(GetAllDriversQuery query, CancellationToken cancellationToken);
    Task<Driver?> Handle(GetDriverByIdQuery query, CancellationToken cancellationToken);
}
