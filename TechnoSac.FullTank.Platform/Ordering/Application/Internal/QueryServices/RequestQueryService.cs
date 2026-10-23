using TechnoSac.FullTank.Platform.Ordering.Application.QueryServices;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Ordering.Application.Internal.QueryServices;

public class RequestQueryService(IRequestRepository repository) : IRequestQueryService
{
    public async Task<IEnumerable<Request>> Handle(GetAllRequestsQuery query, CancellationToken cancellationToken)
    {
        if (query.BuyerCompanyId is not null)
            return await repository.FindByBuyerCompanyIdAsync(query.BuyerCompanyId.Value, cancellationToken);
        if (query.ProviderId is not null)
            return await repository.FindByProviderIdAsync(query.ProviderId.Value, cancellationToken);
        return await repository.ListAsync(cancellationToken);
    }

    public async Task<Request?> Handle(GetRequestByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
