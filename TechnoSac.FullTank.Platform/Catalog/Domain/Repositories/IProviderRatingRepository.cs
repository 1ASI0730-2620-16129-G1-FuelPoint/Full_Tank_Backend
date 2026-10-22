using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Catalog.Domain.Repositories;

public interface IProviderRatingRepository : IBaseRepository<ProviderRating>
{
    Task<IEnumerable<ProviderRating>> FindAsync(int? companyId, int? providerId,
        CancellationToken cancellationToken);

    Task<ProviderRating?> FindByCompanyAndProviderAsync(int companyId, int providerId,
        CancellationToken cancellationToken);
}
