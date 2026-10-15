using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;

public interface IOrderRepository : IBaseRepository<Order>
{
    Task<IEnumerable<Order>> FindByBuyerCompanyIdAsync(int buyerCompanyId, CancellationToken cancellationToken);
    Task<IEnumerable<Order>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken);
    Task<Order?> FindByRequestIdAsync(int requestId, CancellationToken cancellationToken);
}
