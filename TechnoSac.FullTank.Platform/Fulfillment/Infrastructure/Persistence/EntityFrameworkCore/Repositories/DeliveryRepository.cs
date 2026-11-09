using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Fulfillment.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class DeliveryRepository(AppDbContext context) : BaseRepository<Delivery>(context), IDeliveryRepository
{
    public async Task<IEnumerable<Delivery>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken)
    {
        return await Context.Set<Delivery>()
            .Where(delivery => delivery.ProviderId == providerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Delivery>> FindByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return await Context.Set<Delivery>()
            .Where(delivery => delivery.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Delivery?> FindActiveByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return await Context.Set<Delivery>()
            .FirstOrDefaultAsync(delivery => delivery.OrderId == orderId && delivery.Status != "delivered",
                cancellationToken);
    }
}
