using TechnoSac.FullTank.Platform.Catalog.Application.QueryServices;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Catalog.Application.Internal.QueryServices;

public class ProviderRatingQueryService(IProviderRatingRepository repository) : IProviderRatingQueryService
{
    public async Task<IEnumerable<ProviderRating>> Handle(GetAllProviderRatingsQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindAsync(query.CompanyId, query.ProviderId, cancellationToken);
    }

    public async Task<ProviderRating?> Handle(GetProviderRatingByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }
}
