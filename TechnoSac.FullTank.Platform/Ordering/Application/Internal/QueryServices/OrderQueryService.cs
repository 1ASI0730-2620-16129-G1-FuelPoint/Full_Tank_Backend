using TechnoSac.FullTank.Platform.Ordering.Application.QueryServices;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Ordering.Application.Internal.QueryServices;

public class OrderQueryService(IOrderRepository repository) : IOrderQueryService
{
    public async Task<IEnumerable<Order>> Handle(GetAllOrdersQuery query, CancellationToken cancellationToken)
    {
        if (query.BuyerCompanyId is not null)
            return await repository.FindByBuyerCompanyIdAsync(query.BuyerCompanyId.Value, cancellationToken);
        if (query.ProviderId is not null)
            return await repository.FindByProviderIdAsync(query.ProviderId.Value, cancellationToken);
        return await repository.ListAsync(cancellationToken);
    }

    public async Task<Order?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
