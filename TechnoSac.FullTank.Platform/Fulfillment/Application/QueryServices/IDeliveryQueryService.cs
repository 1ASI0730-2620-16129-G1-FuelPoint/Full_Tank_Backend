using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;

public interface IDeliveryQueryService
{
    Task<IEnumerable<Delivery>> Handle(GetAllDeliveriesQuery query, CancellationToken cancellationToken);
    Task<Delivery?> Handle(GetDeliveryByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Delivery>> Handle(GetDeliveriesByOrderIdQuery query, CancellationToken cancellationToken);
}
