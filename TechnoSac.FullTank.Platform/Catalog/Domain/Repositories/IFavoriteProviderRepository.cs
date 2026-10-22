using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;

public interface IFavoriteProviderRepository : IBaseRepository<FavoriteProvider>
{
    Task<IEnumerable<FavoriteProvider>> FindByCompanyIdAsync(int companyId, CancellationToken cancellationToken);
    Task<FavoriteProvider?> FindByCompanyAndProviderAsync(int companyId, int providerId,
        CancellationToken cancellationToken);
}
