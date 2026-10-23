using TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Fulfillment.Application.Internal.QueryServices;

public class DriverQueryService(IDriverRepository repository) : IDriverQueryService
{
    public async Task<IEnumerable<Driver>> Handle(GetAllDriversQuery query, CancellationToken cancellationToken)
    {
        return query.ProviderId is null
            ? await repository.ListAsync(cancellationToken)
            : await repository.FindByProviderIdAsync(query.ProviderId.Value, cancellationToken);
    }

    public async Task<Driver?> Handle(GetDriverByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
