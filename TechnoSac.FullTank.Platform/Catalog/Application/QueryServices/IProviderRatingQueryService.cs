using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Catalog.Application.QueryServices;

public interface IProviderRatingQueryService
{
    Task<IEnumerable<ProviderRating>> Handle(GetAllProviderRatingsQuery query,
        CancellationToken cancellationToken);

    Task<ProviderRating?> Handle(GetProviderRatingByIdQuery query, CancellationToken cancellationToken);
}
