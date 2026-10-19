using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Ordering.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class OrderRepository(AppDbContext context) : BaseRepository<Order>(context), IOrderRepository
{
    public async Task<IEnumerable<Order>> FindByBuyerCompanyIdAsync(int buyerCompanyId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<Order>()
            .Where(order => order.BuyerCompanyId == buyerCompanyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> FindByProviderIdAsync(int providerId, CancellationToken cancellationToken)
    {
        return await Context.Set<Order>()
            .Where(order => order.ProviderId == providerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> FindByRequestIdAsync(int requestId, CancellationToken cancellationToken)
    {
        return await Context.Set<Order>()
            .FirstOrDefaultAsync(order => order.RequestId == requestId, cancellationToken);
    }
}
