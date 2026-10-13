using TechnoSac.FullTank.Platform.Catalog.Application.QueryServices;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Catalog.Application.Internal.QueryServices;

public class ProviderProductQueryService(IProviderProductRepository repository) : IProviderProductQueryService
{
    public async Task<IEnumerable<ProviderProduct>> Handle(GetAllProviderProductsQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }

    public async Task<ProviderProduct?> Handle(GetProviderProductByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
