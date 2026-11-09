using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;

public interface IRequestRepository : IBaseRepository<Request>
{
    Task<IEnumerable<Request>> FindByBuyerCompanyIdAsync(int buyerCompanyId, CancellationToken cancellationToken);
    Task<IEnumerable<Request>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken);
}
