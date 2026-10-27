using TechnoSac.FullTank.Platform.Notification.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using TechnoSac.FullTank.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using NotificationAggregate = TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates.Notification;

namespace TechnoSac.FullTank.Platform.Notification.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class NotificationRepository(AppDbContext context)
    : BaseRepository<NotificationAggregate>(context), INotificationRepository
{
    public async Task<IEnumerable<NotificationAggregate>> FindByBuyerCompanyIdAsync(int buyerCompanyId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<NotificationAggregate>()
            .Where(notification => notification.BuyerCompanyId == buyerCompanyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NotificationAggregate>> FindByProviderIdAsync(int providerId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<NotificationAggregate>()
            .Where(notification => notification.ProviderId == providerId)
            .ToListAsync(cancellationToken);
    }
}
