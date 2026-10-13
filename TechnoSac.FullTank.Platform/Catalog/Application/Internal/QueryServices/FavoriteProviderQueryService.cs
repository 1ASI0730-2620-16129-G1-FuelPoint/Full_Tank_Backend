using TechnoSac.FullTank.Platform.Catalog.Application.QueryServices;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Catalog.Application.Internal.QueryServices;

public class FavoriteProviderQueryService(IFavoriteProviderRepository repository) : IFavoriteProviderQueryService
{
    public async Task<IEnumerable<FavoriteProvider>> Handle(GetAllFavoriteProvidersQuery query,
        CancellationToken cancellationToken)
    {
        return query.CompanyId is null
            ? await repository.ListAsync(cancellationToken)
            : await repository.FindByCompanyIdAsync(query.CompanyId.Value, cancellationToken);
    }

    public async Task<FavoriteProvider?> Handle(GetFavoriteProviderByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
