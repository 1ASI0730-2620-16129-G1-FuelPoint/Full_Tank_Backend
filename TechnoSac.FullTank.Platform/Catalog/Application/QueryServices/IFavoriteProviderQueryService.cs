using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Queries;

namespace TechnoSac.FullTank.Platform.Catalog.Application.QueryServices;

public interface IFavoriteProviderQueryService
{
    Task<IEnumerable<FavoriteProvider>> Handle(GetAllFavoriteProvidersQuery query, CancellationToken cancellationToken);
    Task<FavoriteProvider?> Handle(GetFavoriteProviderByIdQuery query, CancellationToken cancellationToken);
}
