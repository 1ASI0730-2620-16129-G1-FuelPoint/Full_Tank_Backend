using TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.Internal.QueryServices;

public class DeliveryQueryService(IDeliveryRepository repository) : IDeliveryQueryService
{
    public async Task<IEnumerable<Delivery>> Handle(GetAllDeliveriesQuery query, CancellationToken cancellationToken)
    {
        return query.ProviderId is null
            ? await repository.ListAsync(cancellationToken)
            : await repository.FindByProviderIdAsync(query.ProviderId.Value, cancellationToken);
    }

    public async Task<Delivery?> Handle(GetDeliveryByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<Delivery>> Handle(GetDeliveriesByOrderIdQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindByOrderIdAsync(query.OrderId, cancellationToken);
    }
}
