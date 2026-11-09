using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Ordering.Application.QueryServices;

public interface IRequestQueryService
{
    Task<IEnumerable<Request>> Handle(GetAllRequestsQuery query, CancellationToken cancellationToken);
    Task<Request?> Handle(GetRequestByIdQuery query, CancellationToken cancellationToken);
}
