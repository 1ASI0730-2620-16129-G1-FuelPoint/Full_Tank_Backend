using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Ordering.Application.QueryServices;

public interface IOrderQueryService
{
    Task<IEnumerable<Order>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken);
    Task<Order?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken);
}
