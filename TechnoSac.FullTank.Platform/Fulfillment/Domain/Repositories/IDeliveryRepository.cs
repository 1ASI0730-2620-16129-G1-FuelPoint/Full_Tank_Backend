using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;

namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;

public interface IDeliveryRepository : IBaseRepository<Delivery>
{
    Task<IEnumerable<Delivery>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken);
    Task<IEnumerable<Delivery>> FindByOrderIdAsync(int orderId, CancellationToken cancellationToken);
    Task<Delivery?> FindActiveByOrderIdAsync(int orderId, CancellationToken cancellationToken);
}
